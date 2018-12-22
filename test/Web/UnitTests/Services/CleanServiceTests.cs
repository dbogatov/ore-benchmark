using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Web.Extensions;
using Web.Models.Data;
using Web.Models.Data.Entities;

namespace Test.Web.UnitTests.Services
{
	public class CleanService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IConfiguration _config;

		public CleanService()
		{
			var services = new ServiceCollection();

			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv
				.SetupGet(environment => environment.EnvironmentName)
				.Returns("Testing");
			var env = mockEnv.Object;

			var mockConfig = new Mock<IConfiguration>();
			mockConfig
				.SetupGet(config => config["Daemon:CleanService:MaxAge"])
				.Returns($"{24 * 60 * 60}");
			_config = mockConfig.Object;

			services.RegisterSharedServices(env, _config);

			_serviceProvider = services.BuildServiceProvider();
		}

		[Fact]
		public async Task ProperlyCleanData()
		{
			// Arrange
			var context = _serviceProvider.GetRequiredService<IDataContext>();

			var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
			var hourAgo = DateTime.UtcNow.AddHours(-1);
			var now = DateTime.UtcNow;

			await context.Simulations.AddRangeAsync(
				new SingleSimulation { Completed = twoDaysAgo },
				new SingleSimulation { Completed = hourAgo },
				new SingleSimulation { Completed = now }
			);

			await context.SaveChangesAsync();

			// Act
			await new global::Web.Services.CleanService(
				new Mock<ILogger<global::Web.Services.CleanService>>().Object,
				context,
				_config
			)
			.CleanDataPointsAsync();

			// Assert
			Assert.Equal(2, await context.Simulations.CountAsync());
		}
	}
}
