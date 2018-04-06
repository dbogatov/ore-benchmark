using System;
using Simulation;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System.ComponentModel.DataAnnotations;
using System.IO;
using OPESchemes;
using DataStructures.BPlusTree;
using System.Linq;

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

		[Option("--queries-type <enum>", Description = "Type of queries (eq. exact match).")]
		public QueriesType QueriesType { get; } = QueriesType.Exact;

		[Option("--data-structure <enum>", Description = "One of [b-plus-tree]. Default b-plus-tree.")]
		public DataStructure DataStruct { get; } = DataStructure.BPlusTree;

		[Option("--ore-scheme <enum>", Description = "One of [no-enc, crypt-db]. Default no-enc.")]
		public OPESchemes.OPESchemes OREScheme { get; } = OPESchemes.OPESchemes.NoEncryption;

		[Option("--verbose", "If present, more verbose output will be generated.", CommandOptionType.NoValue)]
		public bool Verbose { get; } = false;

		[Range(2, 100)]
		[Option("--b-plus-tree-branches <number>", Description = "Max number of leaves (b parameter) of B+ tree. Must be from 2 to 100. Default 3.")]
		public int BPlusTreeBranching { get; } = 3;

		private int OnExecute()
		{
			Console.WriteLine($"Inputs: dataset={Dataset}, queries={Queries}, type={QueriesType}, dataStructure={DataStruct}, scheme={OREScheme}");

			var readTimer = System.Diagnostics.Stopwatch.StartNew();

			// TODO types hard-coded
			var reader = new DataReader<int, string>(Dataset, Queries, QueriesType);

			readTimer.Stop();

			Console.WriteLine($"Dataset of  {reader.Inputs.Dataset.Count} records.");
			Console.WriteLine($"Queries of  {reader.Inputs.QueriesCount()} queries.");
			Console.WriteLine($"Inputs read in {readTimer.ElapsedMilliseconds} ms");

			var simulator = new Simulator<int, string>(
				reader.Inputs,
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
					BPlusTreeBranching
				)
			);
			var report = simulator.Simulate();

			System.Console.WriteLine(report);

			return 0;
		}
	}
}
