using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CLI;
using McMaster.Extensions.CommandLineUtils;
using ORESchemes.Shared.Primitives;
using Simulation.PureSchemes;

namespace Schemes
{
	[HelpOption("-?|-h|--help")]
	[Command(Name = "schemes-sim", Description = "Scheme simulation utility", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Required]
		[DirectoryExists]
		[Option("--data-dir <DIR>", Description = "Required. Path to data directory.")]
		public string DataDir { get; }

		[Option("--seed <number>", Description = "Seed to use for all operations. Default random (depends on system time).")]
		public int Seed { get; } = new Random().Next();

		[Option("--runs <number>", Description = "Number of runs to perform. Default 10.")]
		public int Runs { get; } = 10;

		[Option("--ore-scheme <enum>", Description = "ORE scheme to use. Default NoEncryption.")]
		public ORESchemes.Shared.ORESchemes OREScheme { get; } = ORESchemes.Shared.ORESchemes.NoEncryption;

		private int OnExecute()
		{
			List<Dictionary<Primitive, long>> primitiveUsagesTotalEncrypt = new List<Dictionary<Primitive, long>>();
			List<Dictionary<Primitive, long>> primitiveUsagesTotalCompare = new List<Dictionary<Primitive, long>>();

			List<Dictionary<Primitive, long>> primitiveUsagesPureEncrypt = new List<Dictionary<Primitive, long>>();
			List<Dictionary<Primitive, long>> primitiveUsagesPureCompare = new List<Dictionary<Primitive, long>>();

			List<long> cipherSizes = new List<long>();
			List<long> stateSizes = new List<long>();

			long encryptActions = 0;
			long compareActions = 0;

			foreach (var dataset in new List<string> { "uniform", "normal", "zipf", "employees", "forest" })
			{
				for (int i = 0; i < Runs; i++)
				{
					var command = new PureSchemeCommand();
					command.Parent = new SimulatorCommand();

					command.Parent.Dataset = Path.Combine(DataDir, $"{dataset}/data.txt");
					command.Parent.ForScript = true;
					command.Parent.OREScheme = OREScheme;
					command.Parent.Seed = Seed;

					Report report = (Report)command.Simulate();

					primitiveUsagesTotalEncrypt.Add(report.Stages[Stages.Encrypt].TotalPrimitiveOperations);
					primitiveUsagesTotalCompare.Add(report.Stages[Stages.Compare].TotalPrimitiveOperations);

					primitiveUsagesPureEncrypt.Add(report.Stages[Stages.Encrypt].PurePrimitiveOperations);
					primitiveUsagesPureCompare.Add(report.Stages[Stages.Compare].PurePrimitiveOperations);

					cipherSizes.Add(((Report.SubReport)report.Stages[Stages.Compare]).MaxCipherSize);
					stateSizes.Add(((Report.SubReport)report.Stages[Stages.Compare]).MaxStateSize);
				
					encryptActions = report.Stages[Stages.Encrypt].SchemeOperations;
					compareActions = report.Stages[Stages.Compare].SchemeOperations;
				}

			}

			Report.SubReport encryptReport = new Report.SubReport();
			Report.SubReport compareReport = new Report.SubReport();

			var primitives = Enum.GetValues(typeof(Primitive)).Cast<Primitive>().OrderBy(v => v);

			AggregateUsage(primitiveUsagesTotalEncrypt, encryptReport.TotalPrimitiveOperations);
			AggregateUsage(primitiveUsagesTotalCompare, compareReport.TotalPrimitiveOperations);
			AggregateUsage(primitiveUsagesPureEncrypt, encryptReport.PurePrimitiveOperations);
			AggregateUsage(primitiveUsagesPureCompare, compareReport.PurePrimitiveOperations);

			void AggregateUsage(List<Dictionary<Primitive, long>> usages, Dictionary<Primitive, long> target)
			{
				foreach (var usage in usages)
				{
					foreach (var primitive in primitives)
					{
						if (!target.ContainsKey(primitive))
						{
							target.Add(primitive, usage[primitive]);
						}
						else
						{
							target[primitive] += usage[primitive];
						}
					}
				}

				var count = usages.Count;

				foreach (var primitive in primitives)
				{
					target[primitive] /= count;
				}
			}

			encryptReport.MaxCipherSize = (long)Math.Round(cipherSizes.Average());
			encryptReport.MaxStateSize = (long)Math.Round(stateSizes.Average());

			compareReport.MaxCipherSize = encryptReport.MaxCipherSize;
			compareReport.MaxStateSize = encryptReport.MaxStateSize;

			encryptReport.SchemeOperations = encryptActions;
			compareReport.SchemeOperations = compareActions;

			Console.WriteLine("\tReport");
			Console.WriteLine("\tEncryption: \n" + encryptReport);
			Console.WriteLine("\tComparison: \n" + compareReport);

			return 0;
		}
	}
}
