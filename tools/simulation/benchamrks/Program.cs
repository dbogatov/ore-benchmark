using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Schemes
{
	enum Type
	{
		Schemes, Primitives
	}

	[HelpOption("-?|-h|--help")]
	[Command(Name = "benchmarks", Description = "Benchmark analyzing simulation utility", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Required]
		[FileExists]
		[Option("--file <DIR>", Description = "Required. Path to the benchmark output file in JSON.")]
		public string ReportFile { get; }

		[Required]
		[Option("--type <enum>", Description = "Required. Type of benchmark - schemes or primitives.")]
		public Type Type { get; }

		private int OnExecute()
		{
			dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(ReportFile));

			Console.WriteLine(data.Benchmarks[0].Statistics.Mean);

			return 0;
		}
	}
}
