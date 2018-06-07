using System;
using Simulation.BPlusTree;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using DataStructures.BPlusTree;
using Simulation;
using CLI.DataReaders;

namespace CLI
{
	[Command(Description = "Simulate a scheme with BPlusTree")]
	public class BPlusTreeCommand : CommandBase
	{
		private SimulatorCommand Parent { get; set; }

		[Required]
		[FileExists]
		[Option("--queries <FILE>", Description = "Required. Path to queries file.")]
		public string Queries { get; }

		[Required]
		[Option("--queries-type <enum>", Description = "Type of queries (eq. Exact).")]
		public QueriesType QueriesType { get; }

		[Range(0, Int32.MaxValue)]
		[Option("--cache-size <number>", Description = "Cache size for node of data structure. 0 means no cache. Default 100.")]
		public int CacheSize { get; } = 100;

		[Range(2, 1024)]
		[Option("--b-plus-tree-branches <number>", Description = "Max number of leaves (b parameter) of B+ tree. Must be from 2 to 1024. Default 3.")]
		public int BPlusTreeBranching { get; } = 3;

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, queries={Queries}, type={QueriesType}, scheme={Parent.OREScheme}, B+tree-branches={BPlusTreeBranching}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new BPlusTree<string>(Parent.Dataset, Queries, QueriesType);
			reader.Inputs.CacheSize = CacheSize;

			timer.Stop();

			PutToConsole($"Dataset of {reader.Inputs.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Queries of {reader.Inputs.QueriesCount()} queries.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			Report report;

			switch (Parent.OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					report =
						new Simulator<string, long>(
							reader.Inputs,
							new Options<long>(
								new NoEncryptionFactory(Parent.Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					report =
						new Simulator<string, long>(
							reader.Inputs,
							new Options<long>(
								new CryptDBOPEFactory(Parent.Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					report =
						new Simulator<string, ORESchemes.PracticalORE.Ciphertext>(
							reader.Inputs,
							new Options<ORESchemes.PracticalORE.Ciphertext>(
								new PracticalOREFactory(Parent.Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					report =
						new Simulator<string, ORESchemes.LewiORE.Ciphertext>(
							reader.Inputs,
							new Options<ORESchemes.LewiORE.Ciphertext>(
								new LewiOREFactory(Parent.Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				default:
					throw new InvalidOperationException($"No such scheme: {Parent.OREScheme}");
			}

			if (!Parent.Verbose)
			{
				System.Console.Write($"{Parent.Seed},{Parent.OREScheme},{QueriesType},{CacheSize},{BPlusTreeBranching},");
			}

			System.Console.WriteLine(Parent.Verbose ? report.ToString() : report.ToConciseString());

			return 0;
		}
	}
}
