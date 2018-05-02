using System;
using System.Collections.Generic;
using System.Linq;

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

	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class Inputs<I, D>
	{
		public List<Record<I, D>> Dataset = new List<Record<I, D>>();
		public QueriesType Type { get; set; }

		public List<ExactQuery<I>> ExactQueries = new List<ExactQuery<I>>();
		public List<RangeQuery<I>> RangeQueries = new List<RangeQuery<I>>();
		public List<UpdateQuery<I, D>> UpdateQueries = new List<UpdateQuery<I, D>>();
		public List<DeleteQuery<I>> DeleteQueries = new List<DeleteQuery<I>>();

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

	public class Report
	{
		public class SubReport
		{
			public int CacheSize { get; set; } = 0;
			public long IOs { get; set; } = 0;
			public long AvgIOs { get; set; } = 0;
			public long SchemeOperations { get; set; } = 0;
			public long AvgSchemeOperations { get; set; } = 0;
			public TimeSpan ObservedTime { get; set; } = new TimeSpan(0);
			public TimeSpan CPUTime { get; set; } = new TimeSpan(0);

			public override string ToString()
			{
				return $@"
		Number of I/O operations (for cache size {CacheSize}): {IOs}
		Average number of I/O operations per query: {AvgIOs}
		Number of OPE/ORE scheme operations performed: {SchemeOperations}
		Average number of OPE/ORE scheme operations per query: {AvgSchemeOperations}
		Observable time elapsed: {ObservedTime}
		CPU time reported: {CPUTime}
";
			}

			/// <summary>
			/// Returns the string representation of the object in a concise manner
			/// </summary>
			/// <param name="queryStage">The stage for which this sub-report was generated</param>
			public string ToConciseString(bool queryStage)
			{
				var stage = queryStage ? "Query" : "Construction";

				return $@"
{stage} CacheSize: {CacheSize}
{stage} IOs: {IOs}
{stage} AvgIOs: {AvgIOs}
{stage} OPs: {SchemeOperations}
{stage} AvgOPs: {AvgSchemeOperations}
{stage} Time: {ObservedTime.TotalMilliseconds}
{stage} CPUTime: {CPUTime.TotalMilliseconds}
";
			}
		}

		public SubReport Construction;
		public SubReport Queries;
		public QueriesType QueriesType;

		public override string ToString()
		{
			return $@"
Report for {QueriesType} queries simulation:
	Construction stage report:
{Construction}
	{QueriesType} queries stage report:
{Queries}
";
		}

		/// <summary>
		/// Returns the string representation of the object in a concise manner
		/// </summary>
		public string ToConciseString()
		{
			return $@"
{Construction.ToConciseString(false)}
{Queries.ToConciseString(true)}
";
		}
	}
}
