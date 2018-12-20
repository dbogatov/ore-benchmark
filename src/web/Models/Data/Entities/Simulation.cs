using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Simulation.Protocol;

namespace Web.Models.Data.Entities
{
	public enum Status
	{
		Pending = 1, InProgress, Completed, Failed
	}

	public class SingleSimulation
	{
		[Key]
		public int Id { get; set; }

		public int? Runner { get; set; } = null;

		public DateTime Created { get; set; } = DateTime.UtcNow;
		public DateTime Started { get; set; } = DateTime.MaxValue;
		public DateTime Completed { get; set; } = DateTime.MaxValue;

		public Status Status { get; set; } = Status.Pending;

		// Inputs
		public int Seed { get; set; } = new Random().Next();
		public ORESchemes.Shared.ORESchemes Protocol { get; set; } = ORESchemes.Shared.ORESchemes.NoEncryption;
		public IList<Record> Dataset { get; set; }
		public IList<RangeQuery> Queryset { get; set; }
		public int CacheSize { get; set; } = 100;

		// Output
		public Report Result { get; set; }
	}
}
