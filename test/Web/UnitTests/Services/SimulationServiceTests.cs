using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Web.Models.Data;
using Web.Extensions;
using Web.Models.Data.Entities;
using System.Linq;
using System.Collections.Generic;
using Simulation.Protocol;

namespace Test.Web.UnitTests.Services
{
	public class SimulationService
	{
		private readonly IConfiguration _config;
		private readonly IDataContext _dataContext;

		public SimulationService()
		{
			var services = new ServiceCollection();

			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv
				.SetupGet(environment => environment.EnvironmentName)
				.Returns("Testing");
			var env = mockEnv.Object;

			var mockConfig = new Mock<IConfiguration>();
			mockConfig
				.SetupGet(config => config["Limits:Queue:Pending"])
				.Returns(3.ToString());
			mockConfig
				.SetupGet(config => config["Limits:Queue:Completed"])
				.Returns(10.ToString());
			mockConfig
				.SetupGet(config => config["Daemon:SimulationService:PageSize"])
				.Returns(8192.ToString());
			_config = mockConfig.Object;

			services.RegisterSharedServices(env, _config);

			var serviceProvider = services.BuildServiceProvider();

			_dataContext = serviceProvider.GetRequiredService<IDataContext>();
		}

		[Fact]
		public async Task EnqueueNormal()
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			await _dataContext.Simulations.AddAsync(new SingleSimulation { Status = Status.Completed });
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.EnqueueAsync(new SingleSimulation());

			// Assert
			Assert.True(result >= 0);

			var simulation = await _dataContext.Simulations.FirstOrDefaultAsync(s => s.Id == result);
			Assert.NotNull(simulation);
			Assert.Equal(Status.Pending, simulation.Status);
			Assert.InRange(
				simulation.Created,
				DateTime.UtcNow - TimeSpan.FromMinutes(1),
				DateTime.UtcNow + TimeSpan.FromMinutes(1)
			);
		}

		[Fact]
		public async Task EnqueueFullPending()
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			await _dataContext.Simulations.AddRangeAsync(
				Enumerable.Range(1, 3).Select(_ => new SingleSimulation { Status = Status.Pending })
			);
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.EnqueueAsync(new SingleSimulation());

			// Assert
			Assert.True(result < 0);
			Assert.Equal(3, await _dataContext.Simulations.CountAsync());
		}

		[Fact]
		public async Task EnqueueFullCompleted()
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			await _dataContext.Simulations.AddRangeAsync(
				Enumerable.Range(1, 10).Select(_ => new SingleSimulation { Status = Status.Completed })
			);
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.EnqueueAsync(new SingleSimulation());

			// Assert
			Assert.True(result < 0);
			Assert.Equal(10, await _dataContext.Simulations.CountAsync());
		}

		[Fact]
		public async Task SimulateNothing()
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			await _dataContext.Simulations.AddAsync(new SingleSimulation { Status = Status.Completed });
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.SimulateAsync();

			// Assert
			Assert.False(result);
		}

		public static IEnumerable<object[]> Protocols =>
			Enum
				.GetValues(typeof(global::ORESchemes.Shared.ORESchemes))
				.Cast<global::ORESchemes.Shared.ORESchemes>()
				.Select(v => new object[] { v });

		[Theory]
		[MemberData(nameof(Protocols))]
		public async Task SimulateSomething(global::ORESchemes.Shared.ORESchemes protocol)
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			var random = new Random(123456);
			await _dataContext.Simulations.AddAsync(
				new SingleSimulation
				{
					Status = Status.Pending,
					Dataset = Enumerable.Range(1, 5).Select(n => new Simulation.Protocol.Record(n, n.ToString())).OrderBy(e => random.Next()).ToList(),
					Queryset = Enumerable.Range(1, 5).Select(n => new RangeQuery(n, n + random.Next(1, 10))).OrderBy(e => random.Next()).ToList(),
					Protocol = protocol
				}
			);
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.SimulateAsync();

			// Assert
			Assert.True(result);
			var simulation = await _dataContext.Simulations.SingleAsync();
			Assert.Equal(Status.Completed, simulation.Status);
			Assert.InRange(
				simulation.Started,
				DateTime.UtcNow - TimeSpan.FromMinutes(1),
				DateTime.UtcNow + TimeSpan.FromMinutes(1)
			);
			Assert.InRange(
				simulation.Completed,
				DateTime.UtcNow - TimeSpan.FromMinutes(1),
				DateTime.UtcNow + TimeSpan.FromMinutes(1)
			);
			Assert.NotNull(simulation.Result);
		}

		[Fact]
		public async Task SimulateFail()
		{
			// Arrange
			var simulationService = new global::Web.Services.SimulationService(
				new Mock<ILogger<global::Web.Services.SimulationService>>().Object,
				_dataContext,
				_config
			);

			var random = new Random(123456);
			await _dataContext.Simulations.AddAsync(
				new SingleSimulation
				{
					Status = Status.Pending,
					Dataset = Enumerable.Range(1, 5).Select(n => new Simulation.Protocol.Record(n, n.ToString())).OrderBy(e => random.Next()).ToList(),
					Queryset = Enumerable.Range(1, 5).Select(n => new RangeQuery(n, n + random.Next(1, 10))).OrderBy(e => random.Next()).ToList(),
					Protocol = global::ORESchemes.Shared.ORESchemes.NoEncryption,
					CacheSize = -1
				}
			);
			await _dataContext.SaveChangesAsync();

			// Act
			var result = await simulationService.SimulateAsync();

			// Assert
			Assert.True(result);
			var simulation = await _dataContext.Simulations.SingleAsync();
			Assert.Equal(Status.Failed, simulation.Status);
			Assert.InRange(
				simulation.Started,
				DateTime.UtcNow - TimeSpan.FromMinutes(1),
				DateTime.UtcNow + TimeSpan.FromMinutes(1)
			);
			Assert.InRange(
				simulation.Completed,
				DateTime.UtcNow - TimeSpan.FromMinutes(1),
				DateTime.UtcNow + TimeSpan.FromMinutes(1)
			);
			Assert.Null(simulation.Result);
		}
	}
}
