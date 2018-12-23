using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Web.Extensions;
using Web.Models.Data;

namespace Web.Services
{
	public interface ICleanService
	{
		/// <summary>
		/// Removes all simulations older than the configured timestamp
		/// </summary>
		/// <param name="maxAge">Time ago earlier than which to consider data too old</param>
		Task CleanDataPointsAsync(TimeSpan? maxAge = null);
	}

	public class CleanService : ICleanService
	{
		private readonly ILogger<CleanService> _logger;
		private readonly IDataContext _context;

		/// <summary>
		/// The maximum age of a simulation
		/// </summary>
		private readonly TimeSpan _maxAge = new TimeSpan();

		public CleanService(
			ILogger<CleanService> logger,
			IDataContext context,
			IConfiguration config
		)
		{
			_logger = logger;
			_context = context;
			_maxAge = new TimeSpan(
				0, 0, Convert.ToInt32(
					config["Daemon:CleanService:MaxAge"]
				)
			);
		}

		public async Task CleanDataPointsAsync(TimeSpan? maxAge = null)
		{
			var toTimestamp = DateTime.UtcNow - (maxAge ?? _maxAge);

			if (await _context.Simulations.AnyAsync(dp => dp.Completed < toTimestamp))
			{
				_context.Simulations.RemoveRange(
					_context.Simulations.Where(dp => dp.Completed < toTimestamp)
				);
			}

			await _context.SaveChangesAsync();

			_logger.LogInformation(LoggingEvents.Clean.AsInt(), "Cleaned old data.");
		}
	}
}
