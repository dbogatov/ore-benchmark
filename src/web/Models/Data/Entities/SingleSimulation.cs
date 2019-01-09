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
			int pageSize,
			ORESchemes.Shared.ORESchemes protocol,
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
								if (record[1].Length > 10)
								{
									throw new ArgumentException($@"
										String must be of maximum 10 characters length. 
										Length {record[1].Length} supplied"
									);
								}
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

			int cipherSize = 0;
			switch (protocol)
			{
				case ORESchemes.Shared.ORESchemes.PracticalORE:
				case ORESchemes.Shared.ORESchemes.CryptDB:
				case ORESchemes.Shared.ORESchemes.FHOPE:
					cipherSize = 64;
					break;
				case ORESchemes.Shared.ORESchemes.ORAM:
				case ORESchemes.Shared.ORESchemes.Florian:
				case ORESchemes.Shared.ORESchemes.POPE:
					cipherSize = 128;
					break;
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					cipherSize = 32;
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					cipherSize = 2816;
					break;
				case ORESchemes.Shared.ORESchemes.AdamORE:
					cipherSize = 4088;
					break;
				case ORESchemes.Shared.ORESchemes.SSE:
					cipherSize = 257;
					break;
			}
			ElementsPerPage = (int)Math.Round((double)pageSize / cipherSize);
			Protocol = protocol;
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
		public int ElementsPerPage { get; set; } = 2;

		// Output
		public Report Result { get; set; }
	}
}
