using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives;

namespace Simulation
{
	/// <typeparam name="S">Stages enum</typeparam>
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

		/// <summary>
		/// Returns a string representing readable printout of primitive usage
		/// </summary>
		protected string PrintPrimitiveUsage()
		{
			string result = "";

			var primitives = Enum.GetValues(typeof(Primitive)).Cast<Primitive>().OrderBy(v => v);

			int padding = primitives.Max(p => p.ToString().Length) + 1;
			int total = "10527220".Length;
			int average = "5623".Length;
			int section = " ( avg)".Length + total + average;

			result += $"\t\t{"Primitive".PadRight(padding)}: {"Total".PadLeft(section)} | {"Pure".PadLeft(section)}{Environment.NewLine}";
			result += "\t\t" + String.Join("", Enumerable.Repeat("-", result.Length - 3)) + Environment.NewLine;

			foreach (var primitive in primitives)
			{
				result += $"\t\t{primitive.ToString().PadRight(padding)}: {TotalPrimitiveOperations[primitive].ToString().PadLeft(total)} ({(TotalPrimitiveOperations[primitive] / SchemeOperations).ToString().PadLeft(average)} avg)";
				result += $" | {PurePrimitiveOperations[primitive].ToString().PadLeft(total)} ({(PurePrimitiveOperations[primitive] / SchemeOperations).ToString().PadLeft(average)} avg){Environment.NewLine}";
			}

			return result;
		}
	}
}
