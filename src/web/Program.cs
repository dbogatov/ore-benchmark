using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Web.Extensions;

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

			await host.RunAsync();

			return 0;
		}
	}
}
