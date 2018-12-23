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

namespace Test.Web.UnitTests.Services
{
	[Trait("Category", "Unit")]
	public class SeedService
	{
		private readonly IConfiguration _config;
		private readonly IDataContext _dataContext;

		public SeedService()
		{
			var services = new ServiceCollection();

			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv
				.SetupGet(environment => environment.EnvironmentName)
				.Returns("Testing");
			var env = mockEnv.Object;

			var mockConfig = new Mock<IConfiguration>();
			mockConfig
				.SetupGet(config => config["Limits:Dataset"])
				.Returns(100.ToString());
			mockConfig
				.SetupGet(config => config["Limits:Queryset"])
				.Returns(1000.ToString());
			_config = mockConfig.Object;

			services.RegisterSharedServices(env, _config);

			var serviceProvider = services.BuildServiceProvider();

			_dataContext = serviceProvider.GetRequiredService<IDataContext>();
		}

		[Fact]
		public void InitiallyNoValues()
		{
			// Assert
			Assert.Empty(_dataContext.Simulations);
		}

		[Fact]
		public async Task ServiceSeedsValuesToDataProvider()
		{
			// Arrange
			var seedService = new global::Web.Services.SeedService(
				_dataContext,
				new Mock<ILogger<global::Web.Services.SeedService>>().Object,
				_config
			);

			// Act
			await seedService.SeedDataAsync();

			// Assert
			Assert.NotEmpty(_dataContext.Simulations);
		}

		[Fact]
		public async Task NoDuplicates()
		{
			// Arrange
			var seedService = new global::Web.Services.SeedService(
				_dataContext,
				new Mock<ILogger<global::Web.Services.SeedService>>().Object,
				_config
			);

			await _dataContext.Simulations.AddAsync(new SingleSimulation());
			await _dataContext.SaveChangesAsync();

			// Act
			await seedService.SeedDataAsync();

			// Assert
			Assert.Equal(1, await _dataContext.Simulations.CountAsync());
		}
	}
}
