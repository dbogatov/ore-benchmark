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
using ORESchemes.AdamORE;

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

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, queries={Queries}, scheme={Parent.OREScheme}, B+tree-branches={BPlusTreeBranching}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new Protocol(Parent.Dataset, Queries);
			reader.Inputs.CacheSize = CacheSize;

			timer.Stop();

			PutToConsole($"Dataset of {reader.Inputs.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Queries of {reader.Inputs.QueriesCount()} queries.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			IProtocol protocol;

			switch (Parent.OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new NoEncryptionFactory().GetScheme(),
								BPlusTreeBranching
							),
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						);
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<CryptDBScheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new CryptDBOPEFactory().GetScheme(),
								BPlusTreeBranching
							),
							new CryptDBOPEFactory(Parent.Seed).GetScheme()
						);
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<PracticalOREScheme, ORESchemes.PracticalORE.Ciphertext, BytesKey>(
						new Options<ORESchemes.PracticalORE.Ciphertext>(
							new PracticalOREFactory().GetScheme(),
							BPlusTreeBranching
						),
						new PracticalOREFactory(Parent.Seed).GetScheme()
					);
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					protocol = new Simulation.Protocol.LewiORE.Protocol(
						new Options<ORESchemes.LewiORE.Ciphertext>(
							new LewiOREFactory().GetScheme(),
							BPlusTreeBranching
						),
						new LewiOREFactory(Parent.Seed).GetScheme()
					);
					break;
				case ORESchemes.Shared.ORESchemes.FHOPE:
					protocol = new Simulation.Protocol.FHOPE.Protocol(
						new Options<ORESchemes.FHOPE.Ciphertext>(
							new FHOPEFactory().GetScheme(),
							BPlusTreeBranching
						),
						new FHOPEFactory(Parent.Seed).GetScheme()
					);
					break;
				case ORESchemes.Shared.ORESchemes.AdamORE:
					protocol = new Simulation.Protocol.SimpleORE.Protocol<AdamOREScheme, ORESchemes.AdamORE.Ciphertext, ORESchemes.AdamORE.Key>(
						new Options<ORESchemes.AdamORE.Ciphertext>(
							new AdamOREFactory().GetScheme(),
							BPlusTreeBranching
						),
						new AdamOREFactory(Parent.Seed).GetScheme()
					);
					break;
				default:
					throw new NotImplementedException($"Scheme {Parent.OREScheme} is not yet supported");
			}

			var report = new Simulator(reader.Inputs, protocol).Simulate();

			System.Console.WriteLine(
				Parent.Verbose ? 
				report.ToString() : 
				JsonReport(
					report.Stages,
					new {
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
