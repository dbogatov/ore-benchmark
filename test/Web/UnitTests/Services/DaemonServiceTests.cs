using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Web.Services;

namespace Test.Web.UnitTests.Services
{
	[Trait("Category", "Unit")]
	public class DaemonService
	{
		private readonly Mock<IServiceProvider> _mockServiceProvider = new Mock<IServiceProvider>();

		private readonly Mock<ISimulationService> _mockSimulationService = new Mock<ISimulationService>();
		private readonly Mock<ICleanService> _mockCleanService = new Mock<ICleanService>();

		private readonly Mock<ILogger<global::Web.Services.DaemonService>> _mockLog = new Mock<ILogger<global::Web.Services.DaemonService>>();
		private readonly Mock<IConfiguration> _config = new Mock<IConfiguration>();

		public DaemonService()
		{
			// Arrange 
			_mockSimulationService
				.Setup(simulation => simulation.SimulateAsync())
				.ReturnsAsync(false);

			_mockServiceProvider
				.Setup(provider => provider.GetService(typeof(ISimulationService)))
				.Returns(_mockSimulationService.Object);
			_mockServiceProvider
				.Setup(provider => provider.GetService(typeof(ICleanService)))
				.Returns(_mockCleanService.Object);

			var mockServiceScope = new Mock<IServiceScope>();
			mockServiceScope
				.SetupGet(scope => scope.ServiceProvider)
				.Returns(_mockServiceProvider.Object);

			var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
			mockServiceScopeFactory
				.Setup(factory => factory.CreateScope())
				.Returns(mockServiceScope.Object);

			_mockServiceProvider
				.Setup(provider => provider.GetService(typeof(IServiceScopeFactory)))
				.Returns(mockServiceScopeFactory.Object);

			foreach (var service in Enum.GetValues(typeof(global::Web.Services.Services)).Cast<global::Web.Services.Services>())
			{
				_config
					.SetupGet(conf => conf[$"Daemon:{service.ToString()}Service:Interval"])
					.Returns(1.ToString());
			}
		}

		[Fact]
		public async Task StartsAndStopsServices()
		{
			// Arrange
			EnableServices();

			var daemonService = new global::Web.Services.DaemonService(
				_mockLog.Object,
				_mockServiceProvider.Object,
				_config.Object
			);

			// Act
			var task = daemonService.StartServices();

			// Let it gracefully start services
			await Task.Delay(1000);

			// Record status
			var waitingForActivation = task.Status;

			// Gracefully stop services
			daemonService.StopServices();

			// Assert

			// Let it finish its job
			// Check that services are stopped
			// If not done in 30 seconds, consider timeout
			Assert.Equal(await Task.WhenAny(task, Task.Delay(new TimeSpan(0, 0, 30))), task);

			Assert.Equal(TaskStatus.WaitingForActivation, waitingForActivation);

			_mockLog
				.Verify(
					log => log.Log(
						LogLevel.Information,
						It.IsAny<EventId>(),
						It.Is<FormattedLogValues>(
							v => v
								.ToString()
								.Contains(
									"All services stopped",
									StringComparison.OrdinalIgnoreCase
								)
						),
						It.IsAny<Exception>(),
						It.IsAny<Func<object, Exception, string>>()
					)
				);
		}

		[Theory]
		[InlineData(global::Web.Services.Services.Simulation)]
		[InlineData(global::Web.Services.Services.Clean)]
		public async Task RunsOnlyIfEnabled(global::Web.Services.Services service)
		{
			// Arrange
			EnableServices(service);

			var daemonService = new global::Web.Services.DaemonService(
				_mockLog.Object,
				_mockServiceProvider.Object,
				_config.Object
			);

			// Act
			var task = daemonService.StartServices();
			await Task.Delay(3000);
			daemonService.StopServices();

			// Let it finish its job
			// Check that services are stopped
			// If not done in 30 seconds, consider timeout
			Assert.Equal(await Task.WhenAny(task, Task.Delay(new TimeSpan(0, 0, 30))), task);

			// Assert

			// Clean
			_mockCleanService.Verify(
				clean => clean.CleanDataPointsAsync(It.IsAny<TimeSpan?>()),
				service == global::Web.Services.Services.Clean ? Times.AtLeastOnce() : Times.Never()
			);

			// Simulate
			_mockSimulationService.Verify(
				simulate => simulate.SimulateAsync(),
				service == global::Web.Services.Services.Simulation ? Times.AtLeastOnce() : Times.Never()
			);

			// Verify run until completion
			_mockLog
				.Verify(
					log => log.Log(
						LogLevel.Information,
						It.IsAny<EventId>(),
						It.Is<FormattedLogValues>(
							v => v
								.ToString()
								.Contains(
									$"{service.ToString()} service run complete",
									StringComparison.OrdinalIgnoreCase
								)
						),
						It.IsAny<Exception>(),
						It.IsAny<Func<object, Exception, string>>()
					)
				);
		}

		[Theory]
		[InlineData(global::Web.Services.Services.Simulation)]
		[InlineData(global::Web.Services.Services.Clean)]
		public async Task FailureDetected(global::Web.Services.Services service)
		{
			// Arrange
			EnableServices(service);

			Exception e = new Exception("error message");

			_mockSimulationService
				.Setup(mock => mock.SimulateAsync())
				.ThrowsAsync(e);

			_mockCleanService
				.Setup(mock => mock.CleanDataPointsAsync(null))
				.ThrowsAsync(e);

			var daemonService = new global::Web.Services.DaemonService(
				_mockLog.Object,
				_mockServiceProvider.Object,
				_config.Object
			);

			// Act
			var task = daemonService.StartServices();
			await Task.Delay(3000);
			daemonService.StopServices();

			// Let it finish its job
			// Check that services are stopped
			// If not done in 30 seconds, consider timeout
			Assert.Equal(await Task.WhenAny(task, Task.Delay(new TimeSpan(0, 0, 30))), task);

			// Assert
			_mockLog
				.Verify(
					log => log.Log(
						LogLevel.Error,
						It.IsAny<EventId>(),
						It.Is<FormattedLogValues>(
							v => v
								.ToString()
								.Contains(
									$"{service.ToString()}",
									StringComparison.OrdinalIgnoreCase
								)
						),
						It.Is<Exception>(ex => ex == e),
						It.IsAny<Func<object, Exception, string>>()
					)
				);
		}

		/// <summary>
		/// Sets up mock config to enable all or a particular service
		/// </summary>
		/// <param name="service">If given, only this service will be enabled</param>
		private void EnableServices(global::Web.Services.Services? service = null)
		{
			if (service.HasValue)
			{
				_config
					.SetupGet(conf => conf[$"Daemon:{service.Value.ToString()}Service:Enabled"])
					.Returns(true.ToString());

				foreach (var toDisable in Enum.GetValues(typeof(global::Web.Services.Services)).Cast<global::Web.Services.Services>())
				{
					if (toDisable != service)
					{
						_config
							.SetupGet(conf => conf[$"Daemon:{toDisable.ToString()}Service:Enabled"])
							.Returns(false.ToString());
					}
				}
			}
			else
			{
				foreach (var toEnable in Enum.GetValues(typeof(global::Web.Services.Services)).Cast<global::Web.Services.Services>())
				{
					_config
						.SetupGet(conf => conf[$"Daemon:{toEnable.ToString()}Service:Enabled"])
						.Returns(true.ToString());
				}
			}
		}
	}
}
