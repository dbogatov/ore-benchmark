using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Web.Extensions;
using Web.Models.Data.Entities;
using Web.Models.Data;
using System.Linq;
using Simulation.Protocol;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
	/// <summary>
	/// Service used to initialize some values in the database. Called each time app starts.
	/// </summary>
	public interface ISeedService
	{
		/// <summary>
		/// Populates data provider with initial set of records (like enums)
		/// </summary>
		Task SeedDataAsync();
	}

	public class SeedService : ISeedService
	{
		private readonly IDataContext _context;
		private readonly ILogger<SeedService> _logger;
		private readonly IConfiguration _configuration;

		public SeedService(
			IDataContext context,
			ILogger<SeedService> logger,
			IConfiguration configuration
		)
		{
			_context = context;
			_logger = logger;
			_configuration = configuration;
		}

		public async Task SeedDataAsync()
		{
			// Put the data into the data provider
			_logger.LogInformation(LoggingEvents.Startup.AsInt(), "DataSeed started");

			if (!(await _context.Simulations.AnyAsync()))
			{
				var random = new Random();

				await _context.Simulations.AddRangeAsync(
					Enumerable
						.Range(1, 3)
						.Select(
							i => new SingleSimulation
							{
								Created = DateTime.UtcNow.AddMinutes(1),
								Dataset = Enumerable.Range(1, 10).Select(n => new Record(n, n.ToString())).OrderBy(e => random.Next()).ToList(),
								Queryset = Enumerable.Range(1, 10).Select(n => new RangeQuery(n, n + random.Next(1, 10))).OrderBy(e => random.Next()).ToList()
							}
						)
						.ToList()
				);
				await _context.SaveChangesAsync();
				
				_logger.LogInformation(LoggingEvents.Startup.AsInt(), "DataSeed finished");
			}
			else
			{
				_logger.LogInformation(LoggingEvents.Startup.AsInt(), "Nothing to seed");
			}
		}
	}
}
