using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Web.Models.Data.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using Simulation.Protocol;

namespace Web.Models.Data
{
	public interface IDataContext : IDisposable
	{
		DbSet<SingleSimulation> Simulations { get; set; }

		DatabaseFacade Database { get; }

		Task<int> SaveChangesAsync(CancellationToken token = default(CancellationToken));
		int SaveChanges();
		void RemoveRange(params object[] entities);
		EntityEntry Remove(object entity);
	}

	/// <summary>
	/// %Models the database. 
	/// DbSet's represent tables. 
	/// See https://msdn.microsoft.com/en-us/library/jj729737(v=vs.113).aspx
	/// </summary>
	public class DataContext : DbContext, IDataContext
	{
		public DataContext(DbContextOptions options) : base(options) { }

		public DbSet<SingleSimulation> Simulations { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Dataset)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
					v => JsonConvert.DeserializeObject<IList<Record>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
				);
				
			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Queryset)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
					v => JsonConvert.DeserializeObject<IList<RangeQuery>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
				);
				
			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Result)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
					v => JsonConvert.DeserializeObject<Report>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
				);

			base.OnModelCreating(builder);
		}
	}
}
