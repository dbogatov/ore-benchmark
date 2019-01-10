using System.Collections.Generic;

namespace Simulation.Protocol
{
	public class RangeQuery
	{
		public int from { get; private set; }
		public int to { get; private set; }

		public RangeQuery(int from, int to)
		{
			this.from = from;
			this.to = to;
		}

		public override string ToString() => $"{{ {from} - {to} }}";
	}

	public class Record
	{
		public int index { get; private set; }
		public string value { get; private set; }

		public Record(int index, string value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString() => $"{{ {index} = \"{value}\" }}";
	}

	public class Inputs
	{
		public List<Record> Dataset = new List<Record>();

		public List<RangeQuery> Queries = new List<RangeQuery>();

		public int CacheSize { get; set; }
		public CachePolicy CachePolicy { get; set; } = CachePolicy.LFU;

		/// <summary>
		/// Returns the number of queries in the inputs.
		/// </summary>
		public int QueriesCount() => Queries.Count;
	}
}
