using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Extensions;
using Xunit;

namespace Test.Web.UnitTests
{
	[Trait("Category", "Unit")]
	public class Logger
	{
		private readonly ServiceProvider _provider;

		public Logger()
		{
			var services = new ServiceCollection();

			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv
				.SetupGet(environment => environment.EnvironmentName)
				.Returns("Testing");

			var mockConfig = new Mock<IConfiguration>();

			services.RegisterSharedServices(mockEnv.Object, mockConfig.Object);

			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

			_provider = services.BuildServiceProvider();

			Console.SetOut(TextWriter.Null);
		}

		public static IEnumerable<object[]> LogLevels =>
			Enum
				.GetValues(typeof(LogLevel))
				.Cast<LogLevel>()
				.Where(l => l != LogLevel.None)
				.Select(v => new object[] { v });

		[Theory]
		[MemberData(nameof(LogLevels))]
		public void LoggerNoExceptions(LogLevel level)
		{
			_provider
				.GetService<ILoggerFactory>()
				.AddExtendedLogger(
					LogLevel.Trace,
					new string[] { }
				);

			var logger = _provider.GetRequiredService<ILogger<Logger>>();

			logger.Log(level, "Hello");
		}

		[Fact]
		public void Exceptions()
		{
			_provider
				.GetService<ILoggerFactory>()
				.AddExtendedLogger(
					LogLevel.Trace,
					new string[] { }
				);

			var logger = _provider.GetRequiredService<ILogger<Logger>>();

			logger.LogError(0, new Exception(), "Error");
		}

		[Fact]
		public void Exclusions()
		{
			_provider
				.GetService<ILoggerFactory>()
				.AddExtendedLogger(
					LogLevel.Trace,
					new string[] {
						"Logger"
					}
				);

			var logger = _provider.GetRequiredService<ILogger<Logger>>();

			logger.LogInformation("Hello");
		}
	}
}
