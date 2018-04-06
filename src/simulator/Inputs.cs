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

	/// <summary>
	/// I - index (plaintext) type
	/// </summary>
	public class ExactQuery<I>
	{
		public I index { get; private set; }

		public ExactQuery(I index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ {index} }}";
		}
	}

	/// <summary>
	/// I - index (plaintext) type
	/// </summary>
	public class RangeQuery<I>
	{
		public I from { get; private set; }
		public I to { get; private set; }

		public RangeQuery(I from, I to)
		{
			this.from = from;
			this.to = to;
		}

		public override string ToString()
		{
			return $"{{ {from} - {to} }}";
		}
	}

	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class UpdateQuery<I, D>
	{
		public I index { get; private set; }
		public D value { get; private set; }

		public UpdateQuery(I index, D value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} <- \"{value}\" }}";
		}
	}

	/// <summary>
	/// I - index (plaintext) type
	/// </summary>
	public class DeleteQuery<I>
	{
		public I index { get; private set; }

		public DeleteQuery(I index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return $"{{ !{index}! }}";
		}
	}

	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class Record<I, D>
	{
		public I index { get; private set; }
		public D value { get; private set; }

		public Record(I index, D value)
		{
			this.index = index;
			this.value = value;
		}

		public override string ToString()
		{
			return $"{{ {index} = \"{value}\" }}";
		}
	}

	public class Inputs<I, D>
	{
		public List<Record<I, D>> Dataset = new List<Record<I, D>>();
		public QueriesType Type { get; set; }

		public List<ExactQuery<I>> ExactQueries = new List<ExactQuery<I>>();
		public List<RangeQuery<I>> RangeQueries = new List<RangeQuery<I>>();
		public List<UpdateQuery<I, D>> UpdateQueries = new List<UpdateQuery<I, D>>();
		public List<DeleteQuery<I>> DeleteQueries = new List<DeleteQuery<I>>();

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
		public class SubReport
		{
			public int IOs { get; set; } = 0;
			public int Time { get; set; } = 0;
			public int CPUCycles { get; set; } = 0;
			public int RAMUsage { get; set; } = 0;

			public override string ToString()
			{
				return $@"
	Number of I/O operations (assuming pages always cached and cash size is infinite): {IOs}
	Number of milliseconds elapsed: {Time}
	Number of CPU cycles elapsed: {CPUCycles}
	Number of megabytes of RAM used: {RAMUsage}
";
			}
		}

		public SubReport Construction;
		public SubReport Queries;
		public QueriesType QueriesType;

		public override string ToString()
		{
			return $@"
Construction stage report:
{Construction}
{QueriesType} queries stage report:
{Queries}
";
		}
	}
}
