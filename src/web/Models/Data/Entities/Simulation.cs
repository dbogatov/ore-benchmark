using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.Data.Entities
{
	public enum Status
	{
		Pending = 1, InProgress, Completed, Failed
	}
	
	public class Simulation
	{
		[Key]
		public int Id { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public Status Status { get; set; } = Status.Pending;
	}
}
