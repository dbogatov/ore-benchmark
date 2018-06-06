using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.BPlusTree
{
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
