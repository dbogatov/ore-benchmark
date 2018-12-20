using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Web.Extensions;
using Web.Models.Data.Entities;
using Web.Models.Data;

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

			await _context.Simulations.AddRangeAsync(
				new List<Simulation> {
					new Simulation { Timestamp = DateTime.UtcNow },
					new Simulation { Timestamp = DateTime.UtcNow.AddMinutes(1) },
					new Simulation { Timestamp = DateTime.UtcNow.AddMinutes(2) }
				}
			);
			await _context.SaveChangesAsync();

			_logger.LogInformation(LoggingEvents.Startup.AsInt(), "DataSeed finished");
		}
	}
}
