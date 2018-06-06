using System.Collections.Generic;
using System.IO;

namespace CLI.DataReaders
{
	public class PureScheme
	{
		public List<int> Dataset = new List<int>();

		/// <summary>
		/// Immediately populates its local dataset list with the data read from file
		/// </summary>
		public PureScheme(string dataset)
		{
			using (StreamReader sr = File.OpenText(dataset))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					Dataset.Add(int.Parse(line));
				}
			}
		}
	}
}
