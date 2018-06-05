using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.PureSchemes
{
	public class Report
	{
		public long SchemeOperations { get; set; } = 0;
		public long AvgSchemeOperations { get; set; } = 0;
		public TimeSpan ObservedTime { get; set; } = new TimeSpan(0);
		public TimeSpan CPUTime { get; set; } = new TimeSpan(0);

		public override string ToString()
		{
			return $@"
		Number of OPE/ORE scheme operations performed: {SchemeOperations}
		Average number of OPE/ORE scheme operations per operation: {AvgSchemeOperations}
		Observable time elapsed: {ObservedTime}
		CPU time reported: {CPUTime}
";
		}

		/// <summary>
		/// Returns the string representation of the object in a concise manner
		/// </summary>
		/// <param name="queryStage">The stage for which this sub-report was generated</param>
		public string ToConciseString()
		{
			return $@"
OPs: {SchemeOperations}
AvgOPs: {AvgSchemeOperations}
Time: {ObservedTime.TotalMilliseconds}
CPUTime: {CPUTime.TotalMilliseconds}
";
		}
	}
}
