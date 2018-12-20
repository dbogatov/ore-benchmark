using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Extensions;
using Web.Models.Data;
using Web.Services;

namespace Web
{
	public class Program
	{
		public async static Task<int> Main(string[] args)
		{
			int port = 80;
			if (args.Length != 0 && (!Int32.TryParse(args[0], out port) || port < 1024 || port > 65534))
			{
				ColoredConsole.WriteLine("Usage: dotnet web.dll [port | number 1024-65534]", ConsoleColor.Red);
				return 1;
			}

			var host = WebHost
				.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.ConfigureLogging(
					logging =>
					{
						logging.ClearProviders();
						logging.AddFilter("Microsoft", LogLevel.None);
					}
				)
				.UseUrls($"http://*:{port}")
				.Build();
				
			RunDaemon();

			await host.RunAsync();

			return 0;
		}

		public async static Task RunDaemon()
		{
			var mockEnv = new Mock<IHostingEnvironment>();
			mockEnv.
				SetupGet(environment => environment.EnvironmentName).
				Returns(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
			var env = mockEnv.Object;

			var services = new ServiceCollection();
			
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false) // read defaults first
				.AddJsonFile(
					$"{(env.IsProduction() ? "/run/secrets/settings/" : "")}appsettings.{env.EnvironmentName.ToLower()}.json",
					optional: env.IsStaging()
				) // override with specific settings file
				.AddJsonFile("version.json", optional: true)
				.AddEnvironmentVariables();
			var configuration = builder.Build();
			
			services.RegisterSharedServices(env, configuration);

			services.AddLogging();

			var provider = services.BuildServiceProvider();
			
			provider
				.GetService<ILoggerFactory>()
				.AddExtendedLogger(
					env.IsTesting() ? LogLevel.Error : configuration["Logging:MinLogLevel"].ToEnum<LogLevel>(),
					configuration.StringsFromArray("Logging:Exclude").ToArray()
				);

			using (var context = provider.GetService<IDataContext>())
			{
				// create scheme if it does not exist
				context.Database.EnsureCreated();
			}

			await provider.GetRequiredService<ISeedService>().SeedDataAsync();

			await provider.GetRequiredService<IDaemonService>().StartServices();
		}
	}
}
