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

	/// <summary>
	/// I - index (plaintext) type
	/// </summary>
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

	/// <summary>
	/// D - data type
	/// </summary>
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

	/// <summary>
	/// I - index (plaintext) type
	/// </summary>
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

	/// <summary>
	/// D - data type
	/// </summary>
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

	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
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
