using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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
		public async static Task<int> Main(string[] args) => await Entrypoint(args);
		
		/// <summary>
		/// A wrapper around Main for easy testing
		/// </summary>
		/// <param name="args">CLI arguments passed from Main</param>
		/// <param name="cancel">Cancellation token to shut down server when needed</param>
		/// <returns>Program return code</returns>
		public async static Task<int> Entrypoint(string[] args, CancellationToken cancel = default(CancellationToken))
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
				
#pragma warning disable CS4014
			RunDaemon();
#pragma warning restore CS4014  

			await host.RunAsync(cancel);

			return 0;
		}

		/// <summary>
		/// Schedule a daemon thread responsible for running iterative services.
		/// Not recommended to be awaited.
		/// </summary>
		private async static Task RunDaemon()
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
					optional: true
				) // override with specific settings file
				.AddJsonFile("version.json", optional: true)
				.AddEnvironmentVariables();
			var configuration = builder.Build();
			
			services.RegisterSharedServices(env, configuration);

			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));

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
