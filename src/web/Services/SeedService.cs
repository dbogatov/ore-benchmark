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
		private readonly IConfiguration _config;

		public SeedService(
			IDataContext context,
			ILogger<SeedService> logger,
			IConfiguration config
		)
		{
			_context = context;
			_logger = logger;
			_config = config;
		}

		public async Task SeedDataAsync()
		{
			// Put the data into the data provider
			_logger.LogInformation(LoggingEvents.Startup.AsInt(), "DataSeed started");

			if (!(await _context.Simulations.AnyAsync()))
			{
				var random = new Random();

				await _context.Simulations.AddAsync(
					 new SingleSimulation
					 {
						 Dataset = Enumerable.Range(1, Convert.ToInt32(_config["Limits:Dataset"])).Select(n => new Record(n, n.ToString())).OrderBy(e => random.Next()).ToList(),
						 Queryset = Enumerable.Range(1, Convert.ToInt32(_config["Limits:Queryset"])).Select(n => new RangeQuery(n, n + random.Next(1, 10))).OrderBy(e => random.Next()).ToList()
					 }
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
