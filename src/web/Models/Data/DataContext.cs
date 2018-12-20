using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Web.Models.Data.Entities;

namespace Web.Models.Data
{
	public interface IDataContext : IDisposable
	{
		DbSet<Simulation> Simulations { get; set; }

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

		public DbSet<Simulation> Simulations { get; set; }
	}
}
