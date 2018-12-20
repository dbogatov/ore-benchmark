using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Web.Extensions;
using Web.Models.Data;
using Web.Services;

namespace Web
{
	public static class Global
	{
		public static int RunnerID = new Random().Next();
		public static readonly InMemoryDatabaseRoot InMemoryDatabaseRoot = new InMemoryDatabaseRoot();
	}

	public class Startup
	{
		/// <summary>
		/// This method gets called by the runtime. Used to build a global configuration object.
		/// </summary>
		/// <param name="env"></param>
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false) // read defaults first
				.AddJsonFile(
					$"{(env.IsProduction() ? "/run/secrets/settings/" : "")}appsettings.{env.EnvironmentName.ToLower()}.json",
					optional: env.IsStaging()
				) // override with specific settings file
				.AddJsonFile("version.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			CurrentEnvironment = env;
		}

		/// <summary>
		/// Global configuration object.
		/// Gets built by Startup method.
		/// </summary>
		public IConfigurationRoot Configuration { get; }

		private IHostingEnvironment CurrentEnvironment { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.RegisterSharedServices(CurrentEnvironment, Configuration);

			services.AddMemoryCache();
			services.AddSession();

			// Add framework services.
			services
				.AddMvc()
				.AddJsonOptions(opt =>
					{
						var resolver = opt.SerializerSettings.ContractResolver;
						if (resolver != null)
						{
							var res = resolver as DefaultContractResolver;
							res.NamingStrategy = null; // this removes the camelcasing
						}
					})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// lowercase all generated url within the app
			services.AddRouting(options => { options.LowercaseUrls = true; });

			// Add Cross Origin Security service
			services.AddCors();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(
			IApplicationBuilder app,
			IHostingEnvironment env,
			ILoggerFactory loggerFactory,
			IServiceProvider serviceProvider
		)
		{
			loggerFactory
				.AddExtendedLogger(
					env.IsTesting() ? LogLevel.Error : Configuration["Logging:MinLogLevel"].ToEnum<LogLevel>(),
					Configuration.StringsFromArray("Logging:Exclude").ToArray()
				);

			if (env.IsProduction())
			{
				app.UseExceptionHandler("/error"); // All serverside exceptions redirect to error page
				app.UseStatusCodePagesWithReExecute("/error/{0}");
			}
			else
			{
				app.UseDatabaseErrorPage();
				app.UseDeveloperExceptionPage(); // Print full stack trace
			}

			app.UseSession();

			app.UseCors(builder => builder.WithOrigins("*"));

			app.UseStaticFiles(); // make accessible and cache wwwroot files
			app.UseDefaultFiles(); // in wwwroot folder, index.html is served when opening a directory

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			using (var context = serviceProvider.GetService<IDataContext>())
			{
				// create scheme if it does not exist
				context.Database.EnsureCreated();
			}

			serviceProvider.GetRequiredService<ISeedService>().SeedDataAsync().Wait();
		}
	}
}
