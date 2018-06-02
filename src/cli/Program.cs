using System;
using Simulation;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using DataStructures.BPlusTree;

namespace CLI
{
	[HelpOption("-?|-h|--help")]
	[Command(Name = "ore-benchamark", Description = "An ORE schemes benchmark", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Required]
		[FileExists]
		[Option("--dataset <FILE>", Description = "Required. Path to dataset file.")]
		public string Dataset { get; }

		[Required]
		[FileExists]
		[Option("--queries <FILE>", Description = "Required. Path to queries file.")]
		public string Queries { get; }

		[Option("--queries-type <enum>", Description = "Type of queries (eq. Exact).")]
		public QueriesType QueriesType { get; } = QueriesType.Exact;

		[Option("--data-structure <enum>", Description = "Data structure to use (eq. BPlusTree)")]
		public DataStructure DataStruct { get; } = DataStructure.BPlusTree;

		[Option("--ore-scheme <enum>", Description = "ORE scheme to use (eq. NoEncryption)")]
		public ORESchemes.Shared.ORESchemes OREScheme { get; } = ORESchemes.Shared.ORESchemes.NoEncryption;

		[Option("--verbose|-v", "If present, more verbose output will be generated.", CommandOptionType.NoValue)]
		public bool Verbose { get; } = false;

		[Range(0, Int32.MaxValue)]
		[Option("--cache-size <number>", Description = "Cache size for node of data structure. 0 means no cache. Default 100.")]
		public int CacheSize { get; } = 100;

		[Range(2, 1024)]
		[Option("--b-plus-tree-branches <number>", Description = "Max number of leaves (b parameter) of B+ tree. Must be from 2 to 1024. Default 3.")]
		public int BPlusTreeBranching { get; } = 3;

		[Option("--seed <number>", Description = "Seed to use for all operations. Default random (depends on system time).")]
		public int Seed { get; } = new Random().Next();

		private int OnExecute()
		{
			Console.WriteLine($"Seed: {Seed}");
			Console.WriteLine($"Inputs: dataset={Dataset}, queries={Queries}, type={QueriesType}, dataStructure={DataStruct}, scheme={OREScheme}");

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new DataReader<string>(Dataset, Queries, QueriesType);
			reader.Inputs.CacheSize = CacheSize;

			timer.Stop();

			Console.WriteLine($"Dataset of {reader.Inputs.Dataset.Count} records.");
			Console.WriteLine($"Queries of {reader.Inputs.QueriesCount()} queries.");
			Console.WriteLine($"Inputs read in {timer.ElapsedMilliseconds} ms.");

			Report report;

			switch (OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					report =
						new Simulator<string, long>(
							reader.Inputs,
							new Options<long>(
								new NoEncryptionFactory(Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					report =
						new Simulator<string, long>(
							reader.Inputs,
							new Options<long>(
								new CryptDBOPEFactory(Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					report =
						new Simulator<string, ORESchemes.PracticalORE.Ciphertext>(
							reader.Inputs,
							new Options<ORESchemes.PracticalORE.Ciphertext>(
								new PracticalOREFactory(Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					report =
						new Simulator<string, ORESchemes.LewiORE.Ciphertext>(
							reader.Inputs,
							new Options<ORESchemes.LewiORE.Ciphertext>(
								new LewiOREFactory(Seed).GetScheme(),
								BPlusTreeBranching
							)
						).Simulate();
					break;
				default:
					throw new InvalidOperationException($"No such scheme: {OREScheme}");
			}

			System.Console.WriteLine(Verbose ? report.ToString() : report.ToConciseString());

			return 0;
		}
	}
}
