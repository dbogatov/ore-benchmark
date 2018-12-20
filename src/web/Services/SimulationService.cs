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

namespace Web.Services
{
	public interface ISimulationService
	{
		Task<bool> SimulateAsync();
	}

	public class SimulationService : ISimulationService
	{
		private readonly ILogger<SimulationService> _logger;
		private readonly IDataContext _context;

		public SimulationService(
			ILogger<SimulationService> logger,
			IDataContext context
		)
		{
			_logger = logger;
			_context = context;
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
								TotalPrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 25 }
								},
								PurePrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 35 }
								},
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
								TotalPrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 251 }
								},
								PurePrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 351 }
								},
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
								TotalPrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 252 }
								},
								PurePrimitiveOperations = new Dictionary<Primitive, long>{
									{ Primitive.AES, 352 }
								},
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
										TotalPrimitiveOperations = new Dictionary<Primitive, long>{
											{ Primitive.AES, 253 }
										},
										PurePrimitiveOperations = new Dictionary<Primitive, long>{
											{ Primitive.AES, 353 }
										},
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
										TotalPrimitiveOperations = new Dictionary<Primitive, long>{
											{ Primitive.AES, 254 }
										},
										PurePrimitiveOperations = new Dictionary<Primitive, long>{
											{ Primitive.AES, 354 }
										},
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
