using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Simulation.Protocol;

namespace Web.Models.Data.Entities
{
	public enum Status
	{
		Pending = 1, InProgress, Completed, Failed
	}

	public class SingleSimulation
	{
		public class MalformedSetException : System.Exception
		{
			public string Set { get; set; }
		}

		public SingleSimulation() { }

		public SingleSimulation(
			string dataset,
			string queryset,
			int datasetSize,
			int querysetSize,
			Random random
		)
		{
			if (string.IsNullOrEmpty(dataset))
			{
				Dataset = Enumerable
					.Range(0, datasetSize)
					.Select(e => random.Next(0, datasetSize / 2))
					.Select(e => new Record(e, e.ToString()))
					.ToList();
			}
			else
			{
				try
				{
					Dataset = new List<Record>();
					var read = 0;
					using (StringReader reader = new StringReader(dataset))
					{
						string line = string.Empty;
						do
						{
							line = reader.ReadLine();
							if (line != null)
							{
								var record = line.Split(',');
								Dataset.Add(new Record(int.Parse(record[0]), record[1]));
								read++;
							}
						} while (line != null && read < datasetSize);
					}
				}
				catch (System.Exception)
				{
					throw new MalformedSetException { Set = "Dataset" };
				}
			}

			if (string.IsNullOrEmpty(queryset))
			{
				Queryset = Enumerable
					.Range(0, querysetSize)
					.Select(e => random.Next(0, (int)Math.Round(datasetSize * 0.4)))
					.Select(n => new RangeQuery(n, n + (int)Math.Round(querysetSize * 0.1)))
					.ToList();
			}
			else
			{
				try
				{
					Queryset = new List<RangeQuery>();
					var read = 0;
					using (StringReader reader = new StringReader(queryset))
					{
						string line = string.Empty;
						do
						{
							line = reader.ReadLine();
							if (line != null)
							{
								var record = line.Split(',');
								Queryset.Add(new RangeQuery(int.Parse(record[0]), int.Parse(record[1])));
								read++;
							}
						} while (line != null && read < querysetSize);
					}
				}
				catch (System.Exception)
				{
					throw new MalformedSetException { Set = "Queryset" };
				}
			}
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public DateTime Created { get; set; } = DateTime.UtcNow;
		public DateTime Started { get; set; } = DateTime.MaxValue;
		public DateTime Completed { get; set; } = DateTime.MaxValue;

		public Status Status { get; set; } = Status.Pending;

		// Inputs
		public int Seed { get; set; } = new Random().Next();
		public ORESchemes.Shared.ORESchemes Protocol { get; set; } = ORESchemes.Shared.ORESchemes.NoEncryption;
		public IList<Record> Dataset { get; set; }
		public IList<RangeQuery> Queryset { get; set; }
		public int CacheSize { get; set; } = 0;

		// Output
		public Report Result { get; set; }
	}
}
