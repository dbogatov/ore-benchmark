using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Protocol
{
	// public class ExactQuery
	// {
	// 	public int index { get; private set; }

	// 	public ExactQuery(int index)
	// 	{
	// 		this.index = index;
	// 	}

	// 	public override string ToString()
	// 	{
	// 		return $"{{ {index} }}";
	// 	}
	// }

	public class RangeQuery
	{
		public int from { get; private set; }
		public int to { get; private set; }

		public RangeQuery(int from, int to)
		{
			this.from = from;
			this.to = to;
		}

		public override string ToString()
		{
			return $"{{ {from} - {to} }}";
		}
	}

	// /// <typeparam name="D">Data type</typeparam>
	// public class UpdateQuery<D>
	// {
	// 	public int index { get; private set; }
	// 	public D value { get; private set; }

	// 	public UpdateQuery(int index, D value)
	// 	{
	// 		this.index = index;
	// 		this.value = value;
	// 	}

	// 	public override string ToString()
	// 	{
	// 		return $"{{ {index} <- \"{value}\" }}";
	// 	}
	// }

	// public class DeleteQuery
	// {
	// 	public int index { get; private set; }

	// 	public DeleteQuery(int index)
	// 	{
	// 		this.index = index;
	// 	}

	// 	public override string ToString()
	// 	{
	// 		return $"{{ !{index}! }}";
	// 	}
	// }

	public class Record
	{
		public int index { get; private set; }
		public string value { get; private set; }

		public Record(int index, string value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} = \"{value}\" }}";
		}
	}

	public class Inputs
	{
		public List<Record> Dataset = new List<Record>();

		public List<RangeQuery> Queries = new List<RangeQuery>();

		public int CacheSize { get; set; }

		/// <summary>
		/// Returns the number of queries in the inputs.
		/// </summary>
		public int QueriesCount() => Queries.Count;
	}
}
