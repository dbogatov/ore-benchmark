using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Web.Models.Data;
using Web.Services;
using Web.Extensions;
using Xunit;

namespace Test.Web.ControllerTests
{
	[Trait("Category", "Unit")]
	public partial class HomeController
	{
		private readonly Mock<ISimulationService> _mockSimulationService = new Mock<ISimulationService>();
		private readonly IConfiguration _config;
		private readonly IDataContext _context;

		private readonly global::Web.Controllers.HomeController _controller;

		public HomeController()
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
				.Returns(10.ToString());
			_config = mockConfig.Object;

			services.RegisterSharedServices(env, _config);

			_context = services
				.BuildServiceProvider()
				.GetRequiredService<IDataContext>();

			_controller = new global::Web.Controllers.HomeController(
				_context,
				_mockSimulationService.Object,
				_config
			);
		}
	}
}
