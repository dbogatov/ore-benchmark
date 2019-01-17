using System;
using System.IO;
using Simulation.Protocol;

namespace CLI.DataReaders
{
	public class Protocol
	{
		public Inputs Inputs = new Inputs();

		/// <summary>
		/// Immediately populates its local lists with the data read from files
		/// </summary>
		public Protocol(string dataset, string queries)
		{
			var random = new Random(123456);
			
			using (StreamReader sr = File.OpenText(dataset))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					var index = int.Parse(line);

					Inputs.Dataset.Add(new Record(index, $"{index}_r{random.Next()}"));
				}
			}

			using (StreamReader sr = File.OpenText(queries))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					var record = line.Split(',');

					Inputs.Queries.Add(new RangeQuery(int.Parse(record[0]), int.Parse(record[1])));
				}
			}
		}
	}
}
