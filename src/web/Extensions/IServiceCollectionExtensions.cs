using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Web.Models.Data;
using Web.Services;
using System;

namespace Web.Extensions
{
	/// <summary>
	/// Utility class for testing methods
	/// </summary>
	public static class IServiceCollectionExtensions
	{
		/// <summary>
		/// Registers all available services for testing environment.
		/// </summary>
		/// <returns>Service provider with all services registered</returns>
		public static IServiceCollection RegisterSharedServices(
			this IServiceCollection services,
			IHostingEnvironment env,
			IConfiguration config)
		{
			services
				.AddEntityFrameworkInMemoryDatabase()
				.AddDbContext<DataContext>(b => b.UseInMemoryDatabase("main-db", Global.InMemoryDatabaseRoot));

			services.AddTransient<IDataContext, DataContext>();

			services.AddTransient<ISeedService, SeedService>();
			services.AddTransient<ICleanService, CleanService>();
			services.AddTransient<IDaemonService, DaemonService>();
			services.AddTransient<ISimulationService, SimulationService>();

			services.AddSingleton<IConfiguration>(config);
			services.AddSingleton<IHostingEnvironment>(env);

			return services;
		}
	}
}
