using System;
using System.Collections.Generic;
using System.Linq;
using OPESchemes;

namespace Simulation
{
	public enum DataStructure
	{
		BPlusTree
	}

	public enum QueriesType
	{
		Exact, Range, Update, Delete
	}

	public class ExactQuery<T>
	{
		public T index { get; private set; }

		public ExactQuery(T index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ {index} }}";
		}
	}

	public class RangeQuery<T>
	{
		public T from { get; private set; }
		public T to { get; private set; }

		public RangeQuery(T from, T to)
		{
			this.from = from;
			this.to = to;
		}

		public override string ToString()
		{
			return $"{{ {from} - {to} }}";
		}
	}

	public class UpdateQuery<T, C>
	{
		public T index { get; private set; }
		public C value { get; private set; }

		public UpdateQuery(T index, C value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} <- \"{value}\" }}";
		}
	}

	public class DeleteQuery<T>
	{
		public T index { get; private set; }

		public DeleteQuery(T index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ !{index}! }}";
		}
	}

	public class Record<T, C>
	{
		public T index { get; private set; }
		public C value { get; private set; }

		public Record(T index, C value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} = \"{value}\" }}";
		}
	}

	public class Inputs<T, C>
	{
		public List<Record<T, C>> Dataset = new List<Record<T, C>>();
		public QueriesType Type { get; set; }

		public List<ExactQuery<T>> ExactQueries = new List<ExactQuery<T>>();
		public List<RangeQuery<T>> RangeQueries = new List<RangeQuery<T>>();
		public List<UpdateQuery<T, C>> UpdateQueries = new List<UpdateQuery<T, C>>();
		public List<DeleteQuery<T>> DeleteQueries = new List<DeleteQuery<T>>();

		public int QueriesCount()
		{
			return new List<int> {
				ExactQueries.Count,
				RangeQueries.Count,
				UpdateQueries.Count,
				DeleteQueries.Count
			}.Max();
		}
	}

	public class Report
	{

	}
}
