using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ORESchemes.Shared.Primitives;

namespace Simulation
{
	public abstract class AbsReport<S> where S : Enum
	{
		public AbsReport()
		{
			foreach (var stage in Enum.GetValues(typeof(S)).Cast<S>())
			{
				Stages.Add(stage, default(AbsSubReport));
			}
		}

		public Dictionary<S, AbsSubReport> Stages = new Dictionary<S, AbsSubReport>();

		public override string ToString()
		{
			var stages = Enum.GetValues(typeof(S)).Cast<S>().OrderBy(v => v);

			string result = "";

			result += "Simulation report\n";

			foreach (var stage in stages)
			{
				result += $@"
	{stage} report:
{Stages[stage]}";
			}

			return result;
		}
	}

	public abstract class AbsSubReport
	{
		public long SchemeOperations { get; set; } = 0;
		public long AvgSchemeOperations { get; set; } = 0;

		public Dictionary<Primitive, long> TotalPrimitiveOperations { get; set; } = new Dictionary<Primitive, long>();
		public Dictionary<Primitive, long> PurePrimitiveOperations { get; set; } = new Dictionary<Primitive, long>();


		public TimeSpan ObservedTime { get; set; } = new TimeSpan(0);
		public TimeSpan CPUTime { get; set; } = new TimeSpan(0);

		protected string PrintPrimitiveUsage()
		{
			string result = "";

			var primitives = Enum.GetValues(typeof(Primitive)).Cast<Primitive>().OrderBy(v => v);

			int padding = primitives.Max(p => p.ToString().Length) + 1;
			int section = "123456 (123 avg)".Length;

			result += $"\t\t{"Primitive".PadRight(padding)}: {"Total".PadLeft(section)} | {"Pure".PadLeft(section)}{Environment.NewLine}";
			result += "\t\t" + String.Join("", Enumerable.Repeat("-", result.Length - 3)) + Environment.NewLine;

			foreach (var primitive in primitives)
			{
				result += $"\t\t{primitive.ToString().PadRight(padding)}: {TotalPrimitiveOperations[primitive].ToString().PadLeft(6)} ({(TotalPrimitiveOperations[primitive] / SchemeOperations).ToString().PadLeft(3)} avg)";
				result += $" | {PurePrimitiveOperations[primitive].ToString().PadLeft(6)} ({(PurePrimitiveOperations[primitive] / SchemeOperations).ToString().PadLeft(3)} avg){Environment.NewLine}";
			}

			return result;
		}
	}
}
