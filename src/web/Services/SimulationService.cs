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

				// TODO fake simulation
				Thread.Sleep(10 * 1000);
				var report = new Report
				{
					Stages = new Dictionary<Stages, Simulation.AbsSubReport> {
						{
							Stages.Handshake,
							new SubReport {
								ActionsNumber = 3,
								SchemeOperations = 4,
								ObservedTime = TimeSpan.FromMinutes(6),
								TotalPrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 15L
										),
								PurePrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 25L
										),
								CacheSize = 50,
								IOs = 5,
								CommunicationVolume = 20,
								MessagesSent = 10,
								MaxClientStorage = 60
							}
						},
						{
							Stages.Construction,
							new SubReport {
								ActionsNumber = 31,
								SchemeOperations = 41,
								ObservedTime = TimeSpan.FromMinutes(61),
								TotalPrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 15L
										),
								PurePrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 25L
										),
								CacheSize = 501,
								IOs = 51,
								CommunicationVolume = 201,
								MessagesSent = 101,
								MaxClientStorage = 601
							}
						},
						{
							Stages.Queries,
							new SubReport {
								ActionsNumber = 32,
								SchemeOperations = 42,
								ObservedTime = TimeSpan.FromMinutes(62),
								TotalPrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 15L
										),
								PurePrimitiveOperations =
									Enum
										.GetValues(typeof(Primitive))
										.Cast<Primitive>()
										.ToDictionary(
											p => p,
											v => 25L
										),
								CacheSize = 502,
								IOs = 52,
								CommunicationVolume = 202,
								MessagesSent = 102,
								MaxClientStorage = 602,
								PerQuerySubreports = new List<Simulation.AbsSubReport>{
									new SubReport {
										ActionsNumber = 33,
										SchemeOperations = 43,
										ObservedTime = TimeSpan.FromMinutes(63),
										TotalPrimitiveOperations =
											Enum
												.GetValues(typeof(Primitive))
												.Cast<Primitive>()
												.ToDictionary(
													p => p,
													v => 15L
												),
										PurePrimitiveOperations =
											Enum
												.GetValues(typeof(Primitive))
												.Cast<Primitive>()
												.ToDictionary(
													p => p,
													v => 25L
										),
										CacheSize = 503,
										IOs = 53,
										CommunicationVolume = 203,
										MessagesSent = 103,
										MaxClientStorage = 603
									},
									new SubReport {
										ActionsNumber = 34,
										SchemeOperations = 44,
										ObservedTime = TimeSpan.FromMinutes(64),
										TotalPrimitiveOperations =
											Enum
												.GetValues(typeof(Primitive))
												.Cast<Primitive>()
												.ToDictionary(
													p => p,
													v => 15L
												),
										PurePrimitiveOperations =
											Enum
												.GetValues(typeof(Primitive))
												.Cast<Primitive>()
												.ToDictionary(
													p => p,
													v => 25L
												),
										CacheSize = 504,
										IOs = 54,
										CommunicationVolume = 204,
										MessagesSent = 104,
										MaxClientStorage = 604
									}
								}
							}
						}
					}
				};

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
