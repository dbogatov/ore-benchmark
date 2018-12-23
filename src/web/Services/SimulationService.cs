using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Web.Extensions;
using Web.Models.Data;
using Web.Models.Data.Entities;
using Simulation.Protocol;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Web.Services
{
	public interface ISimulationService
	{
		/// <summary>
		/// Grab a pending simulation from the queue and run it.
		/// Update simulation entity in data provider afterwards.
		/// </summary>
		/// <returns>Whether a simulation was run</returns>
		Task<bool> SimulateAsync();
		
		/// <summary>
		/// Put a simulation into queue
		/// </summary>
		/// <param name="simulation">Simulation to put to queue</param>
		/// <returns>Non-negative simulation ID if enqueued, or negative error number if limits exceeded</returns>
		Task<int> EnqueueAsync(SingleSimulation simulation);
	}

	public class SimulationService : ISimulationService
	{
		private readonly ILogger<SimulationService> _logger;
		private readonly IDataContext _context;
		private readonly IConfiguration _config;

		public SimulationService(
			ILogger<SimulationService> logger,
			IDataContext context,
			IConfiguration config
		)
		{
			_logger = logger;
			_context = context;
			_config = config;
		}

		public async Task<int> EnqueueAsync(SingleSimulation simulation)
		{
			var pending = await _context.Simulations.CountAsync(s => s.Status == Status.Pending);
			var all = await _context.Simulations.CountAsync();

			_logger.LogInformation(LoggingEvents.Simulation.AsInt(), $"{pending} of {all} elements enqueued.");

			if (
				pending < Convert.ToInt32(_config["Limits:Queue:Pending"]) &&
				all < Convert.ToInt32(_config["Limits:Queue:Completed"])
			)
			{
				await _context.Simulations.AddRangeAsync(new List<SingleSimulation> { simulation });
				await _context.SaveChangesAsync();

				_logger.LogInformation(LoggingEvents.Simulation.AsInt(), $"Simulation {simulation.Id} enqueued.");

				return simulation.Id;
			}
			else
			{
				_logger.LogWarning(LoggingEvents.Simulation.AsInt(), $"Simulation has not been enqueued. The queue is full.");

				return -1;
			}
		}

		public async Task<bool> SimulateAsync()
		{
			_logger.LogInformation(LoggingEvents.Simulation.AsInt(), "Simulation service looking for pending simulations...");
			if (await _context.Simulations.AnyAsync(s => s.Status == Status.Pending))
			{
				var simulation = await _context
					.Simulations
					.Where(s => s.Status == Status.Pending)
					.OrderBy(s => s.Created)
					.FirstAsync();

				simulation.Started = DateTime.UtcNow;
				simulation.Status = Status.InProgress;

				await _context.SaveChangesAsync();

				var pageSize = Convert.ToInt32(_config["Daemon:SimulationService:PageSize"]);
				int cipherSize = 0;
				switch (simulation.Protocol)
				{
					case ORESchemes.Shared.ORESchemes.PracticalORE:
					case ORESchemes.Shared.ORESchemes.CryptDB:
					case ORESchemes.Shared.ORESchemes.FHOPE:
						cipherSize = 64;
						break;
					case ORESchemes.Shared.ORESchemes.Florian:
					case ORESchemes.Shared.ORESchemes.POPE:
						cipherSize = 128;
						break;
					case ORESchemes.Shared.ORESchemes.NoEncryption:
						cipherSize = 32;
						break;
					case ORESchemes.Shared.ORESchemes.LewiORE:
						cipherSize = 2816;
						break;
					case ORESchemes.Shared.ORESchemes.AdamORE:
						cipherSize = 4088;
						break;
				}
				var branches = (int)Math.Round((double)pageSize / cipherSize);

				Report report = null;
				try
				{
					report = (Report)new Simulator(
						new Inputs
						{
							Queries = simulation.Queryset.ToList(),
							Dataset = simulation.Dataset.ToList(),
							CacheSize = simulation.CacheSize
						},
						Simulator.GenerateProtocol(simulation.Protocol, simulation.Seed, branches)
					).Simulate();

					simulation.Status = Status.Completed;
				}
				catch (System.Exception)
				{
					simulation.Status = Status.Failed;
				}
				simulation.Result = report;
				simulation.Completed = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation(LoggingEvents.Simulation.AsInt(), "Simulation completed.");
				
				return true;
			}
			else
			{
				_logger.LogInformation(LoggingEvents.Simulation.AsInt(), "No pending simulations.");

				return false;
			}
		}
	}
}
