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

			switch (Type)
			{
				case Type.Schemes:
					Schemes(data);
					break;
				case Type.Primitives:
					Primitives(data);
					break;
			}

			return 0;
		}

		private void Primitives(dynamic data)
		{
			List<(string, string)> primitives = new List<(string, string)> {
				( "SymmetricEnc", "size=128" ),
				( "PRGCachedNext", string.Empty ),
				( "PRF", "size=128" ),
				( "Hash", "size=128" ),
				( "PRPNoInv", "size=2" ),
				( "SamplerCachedHG", "population=184467440737095516" )
			};

			foreach (var primitive in primitives)
			{
				foreach (dynamic benchmark in data.Benchmarks)
				{
					if (
						benchmark.Method == primitive.Item1 && 
						(primitive.Item2 == string.Empty ? true : ((string)(benchmark.Parameters)).Contains(primitive.Item2)) &&
						!((string)(benchmark.Parameters)).Contains("4096")
					)
					{
						Console.WriteLine(Math.Round((double)benchmark.Statistics.Mean) * 0.001);
					}
				}
			}
		}

		private void Schemes(dynamic data)
		{
			List<(string, int)> schemes = new List<(string, int)> {
				( "BCLO", 48 ),
				( "CLWW", 0 ),
				( "LewiWu", 16 ),
				( "LewiWu", 8 ),
				( "LewiWu", 4 ),
				( "FHOPE", 0 )
			};

			List<string> stages = new List<string> { "Encrypt", "Compare" };

			foreach (var stage in stages)
			{
				foreach (var scheme in schemes)
				{
					foreach (dynamic benchmark in data.Benchmarks)
					{
						if (benchmark.Method == stage && ((string)(benchmark.Parameters)).Contains($"Scheme=({scheme.Item1}, {scheme.Item2})"))
						{
							Console.WriteLine(Math.Round((double)benchmark.Statistics.Mean) * 0.001);
						}
					}
				}
			}
		}
	}
}
