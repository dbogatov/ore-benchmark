using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.BPlusTree
{
	public enum QueriesType
	{
		Exact, Range, Update, Delete
	}

	public class ExactQuery
	{
		public int index { get; private set; }

		public ExactQuery(int index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ {index} }}";
		}
	}

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

	/// <typeparam name="D">Data type</typeparam>
	public class UpdateQuery<D>
	{
		public int index { get; private set; }
		public D value { get; private set; }

		public UpdateQuery(int index, D value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} <- \"{value}\" }}";
		}
	}

	public class DeleteQuery
	{
		public int index { get; private set; }

		public DeleteQuery(int index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ !{index}! }}";
		}
	}

	/// <typeparam name="D">Data type</typeparam>
	public class Record<D>
	{
		public int index { get; private set; }
		public D value { get; private set; }

		public Record(int index, D value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} = \"{value}\" }}";
		}
	}

	/// <typeparam name="D">Data type</typeparam>
	public class Inputs<D>
	{
		public List<Record<D>> Dataset = new List<Record<D>>();
		public QueriesType Type { get; set; }

		public List<ExactQuery> ExactQueries = new List<ExactQuery>();
		public List<RangeQuery> RangeQueries = new List<RangeQuery>();
		public List<UpdateQuery<D>> UpdateQueries = new List<UpdateQuery<D>>();
		public List<DeleteQuery> DeleteQueries = new List<DeleteQuery>();

		public int CacheSize { get; set; }

		/// <summary>
		/// Returns the number of queries in the inputs.
		/// </summary>
		public int QueriesCount()
		{
			switch (Type)
			{
				case QueriesType.Exact:
					return ExactQueries.Count;
				case QueriesType.Range:
					return RangeQueries.Count;
				case QueriesType.Update:
					return UpdateQueries.Count;
				case QueriesType.Delete:
					return DeleteQueries.Count;
				default:
					throw new InvalidOperationException($"Invalid type: {Type}");
			}
		}
	}
}
