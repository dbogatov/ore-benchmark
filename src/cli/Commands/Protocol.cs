using System;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using CLI.DataReaders;
using Simulation.Protocol;
using System.Linq;

namespace CLI
{
	[Command(Description = "Simulate a client-server protocol using an ORE scheme and a B+ tree")]
	public class ProtocolCommand : CommandBase
	{
		private SimulatorCommand Parent { get; set; }

		[Required]
		[FileExists]
		[Option("--queries <FILE>", Description = "Required. Path to queries file.")]
		public string Queries { get; }

		[Range(0, Int32.MaxValue)]
		[Option("--cache-size <number>", Description = "Cache size for node of data structure. 0 means no cache. Default 100.")]
		public int CacheSize { get; } = 100;

		[Range(2, 1024)]
		[Option("--b-plus-tree-branches <number>", Description = "Max number of leaves (b parameter) of B+ tree. Must be from 2 to 1024. Default 3.")]
		public int BPlusTreeBranching { get; } = 3;

		[Range(1, 100)]
		[Option("--data-percent <number>", Description = "The fraction (percent) of data to consume for simulation. Default 100 (all the data).")]
		public int DataPercent { get; } = 100;

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, queries={Queries}, scheme={Parent.OREScheme}, B+tree-branches={BPlusTreeBranching}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new Protocol(Parent.Dataset, Queries);
			reader.Inputs.CacheSize = CacheSize;
			if (DataPercent != 100)
			{
				reader.Inputs.Dataset = reader.Inputs.Dataset.Take((int)Math.Round(reader.Inputs.Dataset.Count * (DataPercent / 100.0))).ToList();
			}

			timer.Stop();

			PutToConsole($"Dataset of {reader.Inputs.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Queries of {reader.Inputs.QueriesCount()} queries.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			IProtocol protocol = Simulator.GenerateProtocol(Parent.OREScheme, Parent.Seed, BPlusTreeBranching);

			var report = new Simulator(reader.Inputs, protocol).Simulate();

			if (!Parent.Extended)
			{
				report.Stages.Values.ToList().ForEach(s => s.PerQuerySubreports.Clear());
			}

			System.Console.WriteLine(
				Parent.Verbose && !Parent.Extended ?
				report.ToString() :
				JsonReport(
					report.Stages,
					new
					{
						BPlusTreeBranching = BPlusTreeBranching,
						CacheSize = CacheSize,
						Queries = Queries,
						Dataset = Parent.Dataset,
						OREScheme = Parent.OREScheme,
						Seed = Parent.Seed
					}
				)
			);

			return 0;
		}
	}
}
