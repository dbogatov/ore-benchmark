using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Web.Extensions;

namespace Web.Services
{
	/// <summary>
	/// Enum of available demonable services
	/// </summary>
	public enum Services
	{
		Clean, Simulation
	}

	/// <summary>
	/// Service used to run some other services as daemons.
	/// </summary>
	public interface IDaemonService
	{
		/// <summary>
		/// Runs all available demonable services.
		/// Does NOT return until services are stopped.
		/// This method is NOT recommended to be awaited.
		/// </summary>
		Task StartServices();

		/// <summary>
		/// Stops all services gracefully.
		/// It may take a few ticks until those services actually stop.
		/// Returns immediately, does NOT wait until services are stopped.
		/// </summary>
		void StopServices();
	}

	public class DaemonService : IDaemonService
	{
		private readonly ILogger<DaemonService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IConfiguration _config;

		/// <summary>
		/// Holds flags whether services should run or stop.
		/// Used by the service itself.
		/// </summary>
		private Dictionary<Services, bool> _status = new Dictionary<Services, bool>() { };

		/// <summary>
		/// Holds intervals to wait for each service.
		/// Interval says how much time to wait between runing the task again.
		/// </summary>
		private Dictionary<Services, TimeSpan> _intervals = new Dictionary<Services, TimeSpan>();

		public DaemonService(
			ILogger<DaemonService> logger,
			IServiceProvider serviceProvider,
			IConfiguration config
		)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_config = config;

			// Sets all services to run.
			// Does NOT run the services.
			var services = Enum.GetValues(typeof(Services)).Cast<Services>();
			foreach (var service in services)
			{
				_intervals[service] = new TimeSpan(
					0,
					0,
					Convert.ToInt32(config[$"Daemon:{service.ToString()}Service:Interval"])
				);
				_status[service] = Convert.ToBoolean(config[$"Daemon:{service.ToString()}Service:Enabled"]);
			}
		}

		/// <summary>
		/// Starts ICleanService service.
		/// Does NOT return until _status is set to false.
		/// </summary>
		private async Task RunCleanServiceAsync()
		{
			_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Clean service started.");

			while (true)
			{
				// Check exit condition
				if (!_status[Services.Clean])
				{
					_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Clean service stopped.");
					break;
				}

				try
				{
					using (var scope = _serviceProvider.CreateScope())
					{
						// Run the task, wait to completion
						await scope
							.ServiceProvider
							.GetRequiredService<ICleanService>()
							.CleanDataPointsAsync();
					}
					// Wait
					Thread.Sleep(_intervals[Services.Clean]);

					_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Clean service run complete");
				}
				catch (System.Exception e)
				{
					_logger.LogError(
						LoggingEvents.Daemon.AsInt(),
						e,
						"Something terribly wrong happend to Clean Service run in Daemon"
					);
					Thread.Sleep(_intervals[Services.Clean]);
				}
			}
		}

		private async Task RunSimulationServiceAsync()
		{
			_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Simulation service started.");

			while (true)
			{
				// Check exit condition
				if (!_status[Services.Simulation])
				{
					_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Simulation service stopped.");
					break;
				}

				try
				{
					bool simulationRun;
					using (var scope = _serviceProvider.CreateScope())
					{
						// Run the task, wait to completion
						simulationRun = await scope
							.ServiceProvider
							.GetRequiredService<ISimulationService>()
							.SimulateAsync();
					}
					// Wait
					if (!simulationRun)
					{
						Thread.Sleep(_intervals[Services.Simulation]);
					}

					_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "Simulation service run complete");
				}
				catch (System.Exception e)
				{
					_logger.LogError(
						LoggingEvents.Daemon.AsInt(),
						e,
						"Something terribly wrong happend to Simulation Service run in Daemon"
					);
					Thread.Sleep(_intervals[Services.Simulation]);
				}
			}
		}

		public async Task StartServices()
		{
			// Define tasks and run them
			var tasks = new List<Task>() {
				Task.Run(RunCleanServiceAsync),
				Task.Run(RunSimulationServiceAsync)
			}.ToArray();

			await Task.WhenAll(tasks);

			_logger.LogInformation(LoggingEvents.Daemon.AsInt(), "All services stopped");
		}

		public void StopServices()
		{
			// Set all statuses to stop
			foreach (var key in _status.Keys.ToList())
			{
				_status[key] = false;
			}
		}
	}
}

