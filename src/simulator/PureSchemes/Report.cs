using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives;

namespace Simulation.PureSchemes
{
	public class Report
	{
		public Dictionary<Primitive, long> TotalPrimitiveOperations { get; set; } = new Dictionary<Primitive, long>();
		public Dictionary<Primitive, long> PurePrimitiveOperations { get; set; } = new Dictionary<Primitive, long>();
		public int OperationsNumber { get; set; }
		public TimeSpan ObservedTime { get; set; } = new TimeSpan(0);
		public TimeSpan CPUTime { get; set; } = new TimeSpan(0);

		public override string ToString() =>
			$@"
	Primitive usage for {OperationsNumber} operations:

{PrintPrimitiveUsage()}
	Observable time elapsed: {ObservedTime}
	CPU time reported: {CPUTime}
";

		/// <summary>
		/// Returns the string representation of the object in a concise manner
		/// </summary>
		/// <param name="queryStage">The stage for which this sub-report was generated</param>
		public string ToConciseString() =>
			$@"{OperationsNumber},{PrintPrimitiveUsageConcise()}{ObservedTime.TotalMilliseconds}{CPUTime.TotalMilliseconds}";

		private string PrintPrimitiveUsage()
		{
			string result = "";

			var primitives = Enum.GetValues(typeof(Primitive)).Cast<Primitive>().OrderBy(v => v);

			int padding = primitives.Max(p => p.ToString().Length) + 1;
			int section = "123456 (123 avg)".Length;

			result += $"\t\t{"Primitive".PadRight(padding)}: {"Total".PadLeft(section)} | {"Pure".PadLeft(section)}{Environment.NewLine}";
			result += "\t\t" + String.Join("", Enumerable.Repeat("-", result.Length - 3)) + Environment.NewLine;

			foreach (var primitive in primitives)
			{
				result += $"\t\t{primitive.ToString().PadRight(padding)}: {TotalPrimitiveOperations[primitive].ToString().PadLeft(6)} ({(TotalPrimitiveOperations[primitive] / OperationsNumber).ToString().PadLeft(3)} avg)";
				result += $" | {PurePrimitiveOperations[primitive].ToString().PadLeft(6)} ({(PurePrimitiveOperations[primitive] / OperationsNumber).ToString().PadLeft(3)} avg){Environment.NewLine}";
			}

			return result;
		}

		private string PrintPrimitiveUsageConcise()
		{
			string result = "";

			var primitives = Enum.GetValues(typeof(Primitive)).Cast<Primitive>().OrderBy(v => v);

			foreach (var primitive in primitives)
			{
				result += $"{TotalPrimitiveOperations[primitive]},{TotalPrimitiveOperations[primitive] / OperationsNumber},";
				result += $"{PurePrimitiveOperations[primitive]},{PurePrimitiveOperations[primitive] / OperationsNumber},";
			}

			return result;
		}
	}
}
