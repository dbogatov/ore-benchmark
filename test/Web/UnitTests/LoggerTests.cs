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
			var services = new ServiceCollection();

			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv
				.SetupGet(environment => environment.EnvironmentName)
				.Returns("Testing");

			var mockConfig = new Mock<IConfiguration>();

			services.RegisterSharedServices(mockEnv.Object, mockConfig.Object);
			
			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

			var provider = services.BuildServiceProvider();

			provider
				.GetService<ILoggerFactory>()
				.AddExtendedLogger(
					LogLevel.Trace,
					new string[] { }
				);

			var logger = provider.GetRequiredService<ILogger<Logger>>();
			
			Console.SetOut(TextWriter.Null);
			logger.Log(level, "Hello");
		}
	}
}
