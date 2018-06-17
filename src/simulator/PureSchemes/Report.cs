using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives;

namespace Simulation.PureSchemes
{
	public enum Stages
	{
		Encrypt, Decrypt, Compare
	}

	public class Report : AbsReport<Stages>
	{
		public class Subreport : AbsSubReport
		{

			public override string ToString() =>
				$@"
		Primitive usage for input of size {SchemeOperations}:

{PrintPrimitiveUsage()}
		Observable time elapsed: {ObservedTime}
		CPU time reported: {CPUTime}
	";

			/// <summary>
			/// Returns the string representation of the object in a concise manner
			/// </summary>
			/// <param name="queryStage">The stage for which this sub-report was generated</param>
			public override string ToConciseString() =>
				$@"{SchemeOperations},{PrintPrimitiveUsageConcise()}{ObservedTime.TotalMilliseconds},{CPUTime.TotalMilliseconds}";
		}
	}
}
