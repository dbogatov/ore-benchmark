using Microsoft.Extensions.Configuration;
using Xunit;

namespace Test.Web.UnitTests
{
	[Trait("Category", "Unit")]
	public class Configuration
	{
		private readonly IConfiguration _config;

		public Configuration()
		{
			// Generating configuration key-value store from file.
			_config =
				new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false)
				.Build();
		}

		[Fact]
		public void Limits()
		{	
			TestIntKey("Limits:Dataset");
			TestIntKey("Limits:Queryset");
			TestIntKey("Limits:Queue:Pending");
			TestIntKey("Limits:Queue:Completed");
		}

		[Theory]
		[InlineData(global::Web.Services.Services.Clean)]
		[InlineData(global::Web.Services.Services.Simulation)]
		public void DaemonCommon(global::Web.Services.Services service)
		{
			TestBoolKey($"Daemon:{service.ToString()}Service:Enabled");
			TestIntKey($"Daemon:{service.ToString()}Service:Interval");
		}

		[Fact]
		public void DaemonSpecific()
		{
			TestIntKey("Daemon:CleanService:MaxAge");
			TestIntKey("Daemon:SimulationService:PageSize");
		}

		private void TestStringKey(string key)
		{
			Assert.NotNull(_config[key]);
			Assert.False(string.IsNullOrWhiteSpace(_config[key]));
		}

		private void TestIntKey(string key)
		{
			Assert.NotNull(_config[key]);
			Assert.True(int.TryParse(_config[key], out _));
		}

		private void TestBoolKey(string key)
		{
			Assert.NotNull(_config[key]);
			Assert.True(bool.TryParse(_config[key], out _));
		}
	}
}
