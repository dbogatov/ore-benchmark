using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Web.Extensions;
using Web.Models.Data;
using Web.Models.Data.Entities;
using System.Threading;
using Simulation.Protocol;
using System.Collections.Generic;
using static Simulation.Protocol.Report;
using ORESchemes.Shared.Primitives;
using Microsoft.Extensions.Configuration;

namespace Web.Services
{
	public interface ISimulationService
	{
		Task<bool> SimulateAsync();
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
			var ahead = await _context.Simulations.CountAsync(s => s.Status == Status.Pending);
			_logger.LogInformation(LoggingEvents.Simulation.AsInt(), $"{ahead} elements enqueued.");

			if (ahead < Convert.ToInt32(_config["Limits:QueueSize"]))
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

				int branches = 0;
				switch (simulation.Protocol)
				{
					case ORESchemes.Shared.ORESchemes.PracticalORE:
					case ORESchemes.Shared.ORESchemes.CryptDB:
					case ORESchemes.Shared.ORESchemes.FHOPE:
						branches = 512;
						break;
					case ORESchemes.Shared.ORESchemes.Florian:
					case ORESchemes.Shared.ORESchemes.POPE:
						branches = 256;
						break;
					case ORESchemes.Shared.ORESchemes.NoEncryption:
						branches = 1024;
						break;
					case ORESchemes.Shared.ORESchemes.LewiORE:
						branches = 11;
						break;
					case ORESchemes.Shared.ORESchemes.AdamORE:
						branches = 8;
						break;
				}

				Report report = (Report)new Simulator(
					new Inputs
					{
						Queries = simulation.Queryset.ToList(),
						Dataset = simulation.Dataset.ToList(),
						CacheSize = simulation.CacheSize
					},
					Simulator.GenerateProtocol(simulation.Protocol, simulation.Seed, branches)
				).Simulate();

				simulation.Result = report;
				simulation.Completed = DateTime.UtcNow;
				simulation.Status = Status.Completed;

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
