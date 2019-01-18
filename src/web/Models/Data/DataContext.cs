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
using Simulation;

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
	/// Models the database. 
	/// DbSet's represent tables. 
	/// See https://msdn.microsoft.com/en-us/library/jj729737(v=vs.113).aspx
	/// </summary>
	public class DataContext : DbContext, IDataContext
	{
		public DataContext(DbContextOptions options) : base(options) { }

		public DbSet<SingleSimulation> Simulations { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.TypeNameHandling = TypeNameHandling.Auto;
			settings.Converters.Add(new AbsSubReportConverter());

			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Dataset)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, settings),
					v => JsonConvert.DeserializeObject<IList<Record>>(v, settings)
				);

			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Queryset)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, settings),
					v => JsonConvert.DeserializeObject<IList<RangeQuery>>(v, settings)
				);

			builder
				.Entity<SingleSimulation>()
				.Property(s => s.Result)
				.HasConversion(
					v => JsonConvert.SerializeObject(v, settings),
					v => JsonConvert.DeserializeObject<Report>(v, settings)
				);

			base.OnModelCreating(builder);
		}

		/// <summary>
		/// Helper class that defines how report should be deserialized
		/// given that the definition include abstract class
		/// </summary>
		class AbsSubReportConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType) =>
				objectType == typeof(AbsSubReport);

			public override object ReadJson(
				JsonReader reader,
				Type objectType,
				object existingValue,
				JsonSerializer serializer
			) =>
				serializer.Deserialize(reader, typeof(Report.SubReport));

			public override void WriteJson(
				JsonWriter writer,
				object value,
				JsonSerializer serializer
			) =>
				serializer.Serialize(writer, value, typeof(Report.SubReport));
		}
	}
}
