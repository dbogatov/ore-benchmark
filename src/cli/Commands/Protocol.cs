using System;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using CLI.DataReaders;
using Simulation.Protocol;
using System.Linq;
using Simulation;

namespace CLI
{
	[Command(Description = "Simulate a client-server range query protocol")]
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
		
		[Option("--cache-policy <enum>", Description = "Cache policy to use for I/O. Default LFU.")]
		public CachePolicy CachePolicy { get; } = CachePolicy.LFU;

		[Range(2, 1024)]
		[Option("--elements-per-page <number>", Description = "Number of elements that fit in a page. Specific to protocol, see docs. Must be from 2 to 1024. Default 3.")]
		public int ElementsPerPage { get; } = 3;

		[Range(1, 100)]
		[Option("--data-percent <number>", Description = "The fraction (percent) of data to consume for simulation. Default 100 (all the data).")]
		public int DataPercent { get; } = 100;

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, queries={Queries}, protocol={Parent.Protocol}, elements-per-page={ElementsPerPage}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new Protocol(Parent.Dataset, Queries);
			reader.Inputs.CacheSize = CacheSize;
			reader.Inputs.CachePolicy = CachePolicy;
			if (DataPercent != 100)
			{
				reader.Inputs.Dataset = reader.Inputs.Dataset.Take((int)Math.Round(reader.Inputs.Dataset.Count * (DataPercent / 100.0))).ToList();
			}

			timer.Stop();

			PutToConsole($"Dataset of {reader.Inputs.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Queries of {reader.Inputs.QueriesCount()} queries.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			IProtocol protocol = Simulator.GenerateProtocol(Parent.Protocol, Parent.Seed, ElementsPerPage);

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
						ElementsPerPage = ElementsPerPage,
						CacheSize = CacheSize,
						Queries = Queries,
						Dataset = Parent.Dataset,
						Protocol = Parent.Protocol,
						Seed = Parent.Seed
					}
				)
			);

			return 0;
		}
	}
}
