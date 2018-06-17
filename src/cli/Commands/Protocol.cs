using System;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using DataStructures.BPlusTree;
using Simulation;
using CLI.DataReaders;
using Simulation.Protocol;
using ORESchemes.PracticalORE;
using ORESchemes.CryptDBOPE;
using ORESchemes.Shared;

namespace CLI
{
	[Command(Description = "Simulate a scheme with BPlusTree")]
	public class ProtocolCommand : CommandBase
	{
		private SimulatorCommand Parent { get; set; }

		[Required]
		[FileExists]
		[Option("--queries <FILE>", Description = "Required. Path to queries file.")]
		public string Queries { get; }

		// [Required]
		// [Option("--queries-type <enum>", Description = "Type of queries (eq. Exact).")]
		// public QueriesType QueriesType { get; }

		[Range(0, Int32.MaxValue)]
		[Option("--cache-size <number>", Description = "Cache size for node of data structure. 0 means no cache. Default 100.")]
		public int CacheSize { get; } = 100;

		[Range(2, 1024)]
		[Option("--b-plus-tree-branches <number>", Description = "Max number of leaves (b parameter) of B+ tree. Must be from 2 to 1024. Default 3.")]
		public int BPlusTreeBranching { get; } = 3;

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, queries={Queries}, scheme={Parent.OREScheme}, B+tree-branches={BPlusTreeBranching}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new BPlusTree(Parent.Dataset, Queries);
			reader.Inputs.CacheSize = CacheSize;

			timer.Stop();

			PutToConsole($"Dataset of {reader.Inputs.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Queries of {reader.Inputs.QueriesCount()} queries.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			IProtocol protocol;

			switch (Parent.OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<NoEncryptionScheme, long, object>(
							new Options<long>(
								new NoEncryptionFactory().GetScheme(),
								BPlusTreeBranching
							),
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						);
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<CryptDBScheme, long, byte[]>(
							new Options<long>(
								new CryptDBOPEFactory().GetScheme(),
								BPlusTreeBranching
							),
							new CryptDBOPEFactory(Parent.Seed).GetScheme()
						);
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<PracticalOREScheme, Ciphertext, byte[]>(
						new Options<ORESchemes.PracticalORE.Ciphertext>(
							new PracticalOREFactory().GetScheme(),
							BPlusTreeBranching
						),
						new PracticalOREFactory(Parent.Seed).GetScheme()
					);
					break;
				// case ORESchemes.Shared.ORESchemes.LewiORE:
				// 	report =
				// 		new Simulator<string, ORESchemes.LewiORE.Ciphertext, ORESchemes.LewiORE.Key>(
				// 			reader.Inputs,
				// 			new LewiOREFactory(Parent.Seed).GetScheme(),
				// 			BPlusTreeBranching
				// 		).Simulate();
				// 	break;
				// case ORESchemes.Shared.ORESchemes.FHOPE:
				// 	report =
				// 		new Simulator<string, ORESchemes.FHOPE.Ciphertext, ORESchemes.FHOPE.State>(
				// 			reader.Inputs,
				// 			new FHOPEFactory(Parent.Seed).GetScheme(),
				// 			BPlusTreeBranching
				// 		).Simulate();
				// 	break;
				default:
					throw new NotImplementedException($"Scheme {Parent.OREScheme} is not yet supported");
			}

			var report = new Simulator(reader.Inputs, protocol).Simulate();

			if (!Parent.Verbose)
			{
				System.Console.Write($"{Parent.Seed},{Parent.OREScheme},{CacheSize},{BPlusTreeBranching},");
			}

			System.Console.WriteLine(Parent.Verbose ? report.ToString() : report.ToConciseString());

			return 0;
		}
	}
}
