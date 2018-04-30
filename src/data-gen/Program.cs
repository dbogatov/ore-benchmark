using System;
using Simulation;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace DataGen
{
	[HelpOption("-?|-h|--help")]
	[Command(Name = "data-gen", Description = "Data generation utility", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Option("--dataset", "If present, dataset will be generated.", CommandOptionType.NoValue)]
		public bool Dataset { get; } = false;

		[Option("--queries-type <enum>", Description = "Type of queries (eq. Exact).")]
		public QueriesType QueriesType { get; } = QueriesType.Exact;

		[Range(10, Int32.MaxValue)]
		[Option("--count <number>", Description = "Number of records/queries to generate. Default 100.")]
		public int Count { get; } = 100;

		[Range(10, Int32.MaxValue)]
		[Option("--max <number>", Description = "Largest index number (can control record/query density). Default 100.")]
		public int Max { get; } = 100;

		[Option("--seed <number>", Description = "Random seed to use for generation. Default one is a current timestamp ticks.")]
		public int Seed { get; } = unchecked((int)DateTime.Now.Ticks);

		private int OnExecute()
		{
			var generator = new Random(Seed + 3 * (13 + (int)QueriesType));

			for (int i = 0; i < Count; i++)
			{
				var first = generator.Next(Max);
				var second = generator.Next(Max);

				if (Dataset)
				{
					Console.WriteLine($"{first},\"{first}\"");
					continue;
				}

				switch (QueriesType)
				{
					case QueriesType.Exact:
					case QueriesType.Delete:
						Console.WriteLine($"{first}");
						break;
					case QueriesType.Update:
						Console.WriteLine($"{first},\"{first}_updated_{second}\"");
						break;
					case QueriesType.Range:
						if (first == second)
						{
							i--;
							continue;
						}
						Console.WriteLine(first < second ? $"{first},{second}" : $"{second},{first}");
						break;
					default:
						throw new InvalidOperationException($"Invalid type: {QueriesType}");
				}
			}

			return 0;
		}
	}
}
