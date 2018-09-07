using System;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using System.Net.Http;
using System.Threading.Tasks;
using FileHelpers;

namespace DataGen
{
	public enum Type
	{
		Uniform, Normal, Zipf, Employees, Forest
	}

	[DelimitedRecord(",")]
	[IgnoreFirst(1)]
	public class Employee
	{
		public string Name;

		[FieldQuoted('"', QuoteMode.OptionalForBoth)]
		public string job;

		public decimal Base;
		public decimal Overtime;
		public decimal Other;
		public decimal Benefits;
		public decimal TotalPay;
		public decimal TotalPayBenefits;

		public int Year;

		[FieldQuoted('"', QuoteMode.OptionalForBoth)]
		public string Note;

		[FieldQuoted('"', QuoteMode.OptionalForBoth)]
		public string Agency;

		[FieldQuoted('"', QuoteMode.OptionalForBoth)]
		public string Status;
	}

	[DelimitedRecord(",")]
	public class Forest
	{
		public int Elevation;
		public int Aspect;
		public int Slope;
		public int HorizontalDistanceToHydrology;
		public int VerticalDistanceToHydrology;
		public int HorizontalDistanceToRoadways;
		public int Hillshade9am;
		public int HillshadeNoon;
		public int Hillshade3pm;
		public int HorizontalDistanceToFirePoints;

		[FieldArrayLength(4)]
		public int[] WildernessArea;

		[FieldArrayLength(40)]
		public int[] SoilType;

		public int CoverType;

	}

	[HelpOption("-?|-h|--help")]
	[Command(Name = "data-gen", Description = "Data generation utility", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static async Task<int> Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>(args);

		[Required]
		[Option("--type <number>", Description = "Type / distribution of data to generate.")]
		public Type Type { get; }

		[Range(10, 1000000)]
		[Option("--data-size <number>", Description = "Size of the dataset to generate. Default 1000.")]
		public int DataSize { get; } = 1000;

		[Range(10, int.MaxValue - 1)]
		[Option("--uniform-range <number>", Description = "Defines the range of the uniform distribution. Eg. value 10 means range [-10, 10]. Default whole int32 range.")]
		public int UniformRange { get; } = int.MaxValue - 1;

		[Range(10, int.MaxValue / 4)]
		[Option("--normal-stddev <number>", Description = "Defines the standard deviation of the normal distribution. Default 1000.")]
		public int NormalStdDev { get; } = 1000;

		[Range(1, double.MaxValue)]
		[Option("--zipf-param <number>", Description = "Defines the Zipf distribution parameter. Default 1.5.")]
		public double ZipfParam { get; } = 1.5;

		[Option("--employees-url <string>", Description = "If Employees type is requested, this is a CSV file's URL.")]
		public string EmployeesUrl { get; } // https://vadim-dolores-space.nyc3.digitaloceanspaces.com/public/state-of-california-2017.csv

		[Option("--forest-url <string>", Description = "If Forest type is requested, this is a CSV file's URL.")]
		public string ForestUrl { get; } // https://vadim-dolores-space.nyc3.digitaloceanspaces.com/public/covtype.data

		[Range(10, 1000000)]
		[Option("--query-size <number>", Description = "Size of the query set to generate. Default 1000.")]
		public int QuerySize { get; } = 1000;

		[Range(0.5, 80)]
		[Option("--query-range <number>", Description = "Generate query ranges of lengths equal to this percent of data range. Default 0.5.")]
		public double QueryRange { get; } = 0.5;

		[Option("--seed <number>", Description = "Random seed to use for generation. Default random (depends on system time).")]
		public int Seed { get; } = new Random().Next();

		[Required]
		[DirectoryExists]
		[Option("--output <DIR>", Description = "Path to output directory.")]
		public string Output { get; }

		private async Task<int> OnExecute()
		{
			Random generator = new Random(Seed);

			List<int> data = new List<int>();
			List<int> queries = new List<int>();

			switch (Type)
			{
				case Type.Uniform:
					var uniform = new DiscreteUniform(-UniformRange, UniformRange, generator);
					for (int i = 0; i < DataSize; i++)
					{
						data.Add(uniform.Sample());
					}
					break;
				case Type.Normal:
					var normal = new Normal(0, NormalStdDev, generator);
					for (int i = 0; i < DataSize; i++)
					{
						data.Add((int)Math.Round(normal.Sample()));
					}
					break;
				case Type.Zipf:
					var zipf = new Zipf(ZipfParam, 1000, generator);
					for (int i = 0; i < DataSize; i++)
					{
						data.Add(zipf.Sample());
					}
					break;
				case Type.Employees:
					if (EmployeesUrl == null)
					{
						throw new ArgumentException("--employees-url must be set to use Employees data set.");
					}
					using (var client = new HttpClient())
					{
						var responseString = await client.GetStringAsync(EmployeesUrl);

						var set = new HashSet<int>();
						var tmpList = new List<int>();
						var engine = new FileHelperAsyncEngine<Employee>();

						using (engine.BeginReadString(responseString))
						{
							foreach (var employee in engine)
							{
								tmpList.Add((int)Math.Round(employee.TotalPayBenefits));
							}
						}

						var count = tmpList.Count();
						for (int i = 0; i < DataSize; i++)
						{
							if (!set.Add(generator.Next(0, count)))
							{
								i--;
								continue;
							}
						}

						foreach (var index in set)
						{
							data.Add(tmpList[index]);
						}
					}
					break;
				case Type.Forest:
					if (ForestUrl == null)
					{
						throw new ArgumentException("--forest-url must be set to use Forest data set.");
					}
					using (var client = new HttpClient())
					{
						var responseString = await client.GetStringAsync(ForestUrl);

						var set = new HashSet<int>();
						var tmpList = new List<int>();
						var engine = new FileHelperAsyncEngine<Forest>();

						using (engine.BeginReadString(responseString))
						{
							foreach (var forest in engine)
							{
								tmpList.Add(forest.Elevation);
							}
						}

						var count = tmpList.Count();
						for (int i = 0; i < DataSize; i++)
						{
							if (!set.Add(generator.Next(0, count)))
							{
								i--;
								continue;
							}
						}

						foreach (var index in set)
						{
							data.Add(tmpList[index]);
						}
					}
					break;
				default:
					throw new ArgumentException();
			}

			data.Shuffle();

			using (StreamWriter sw = new StreamWriter(Path.Combine(Output, "data.txt")))
			{
				foreach (var point in data)
				{
					await sw.WriteLineAsync($"{point},\"{point.ToString()}\"");
				}
			}

			int min = data.Min();
			int max = data.Max();

			var range = Math.Ceiling((max - min) * QueryRange / 100);
			var sampler = new DiscreteUniform(min, max, generator);

			using (StreamWriter sw = new StreamWriter(Path.Combine(Output, $"queries-{QueryRange.ToString("#.#")}.txt")))
			{
				for (int i = 0; i < QuerySize; i++)
				{
					var first = sampler.Sample();

					if (first >= max - range)
					{
						i--;
						continue;
					}
					await sw.WriteLineAsync($"{first},{first + range}");
				}
			}

			return 0;
		}
	}

	public static class Extensions
	{
		// https://stackoverflow.com/a/1262619/1644554
		public static void Shuffle<T>(this IList<T> list, int? seed = null)
		{
			Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
