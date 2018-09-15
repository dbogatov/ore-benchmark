using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Schemes
{
	enum Type
	{
		Table, Plots, QuerySizes, DataPercent
	}

	public enum Stage
	{
		Construction, Queries
	}

	public class AllReports
	{
		// Reports[protocol][distribution][query-range][data-percent]
		public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Report>>>> Reports = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Report>>>>();

		public class Report
		{
			public class Subreport
			{
				public long IOs { get; set; } = 0;
				public long CommunicationVolume { get; set; } = 0;
				public long CommunicationSize { get; set; } = 0;
			}

			public Dictionary<Stage, Subreport> Stages = new Dictionary<Stage, Subreport>();
		}
	}

	[HelpOption("-?|-h|--help")]
	[Command(Name = "protocols-sim", Description = "Protocol simulation utility", ThrowOnUnexpectedArgument = true)]
	class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static async Task<int> Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>(args);

		[Required]
		[DirectoryExists]
		[Option("--data <DIR>", Description = "Required. Path to the directory with protocol output files in JSON.")]
		public string ReportDir { get; }

		[DirectoryExists]
		[Option("--output <DIR>", Description = "Path to the output directory for plots type.")]
		public string Output { get; }

		[Required]
		[Option("--tail <string>", Description = "Required. Tail (after protocol, distro and query-range) part of file names {protocol}-{distro}-{query-range}-{cache}-{seed}.json.")]
		public string Tail { get; }

		[Option("--query-range <string>", Description = "For types that are not QuerySize, query range for which simulation was run.")]
		public string QueryRange { get; } = "1";

		[Option("--data-percent <string>", Description = "For types that are not DataPercent, query range for which simulation was run.")]
		public string DataPercent { get; } = "100";

		[Option("--distro <string>", Description = "Distribution for which simulation was run. Required for QuerySizes type.")]
		public string Distro { get; }

		[Required]
		[Option("--type <enum>", Description = "Required. Type of output - table or plots.")]
		public Type Type { get; }

		private async Task<int> OnExecute()
		{
			switch (Type)
			{
				case Type.Plots:
					if (Output == null)
					{
						throw new ArgumentException("Output must be set.");
					}
					break;
				case Type.QuerySizes:
				case Type.DataPercent:
					if (Output == null)
					{
						throw new ArgumentException("Output must be set.");
					}
					if (Distro == null)
					{
						throw new ArgumentException("Distro must be set.");
					}
					break;
			}

			AllReports reports = new AllReports();

			foreach (var protocol in new List<string> { "adamore", "cryptdb", "practicalore", "lewiore", "fhope", "florian", "pope", "noencryption", "popecold" })
			{
				reports.Reports.Add(protocol, new Dictionary<string, Dictionary<string, Dictionary<string, AllReports.Report>>>());

				foreach (var distribution in Type == Type.QuerySizes || Type == Type.DataPercent ? new List<string> { Distro } : new List<string> { "uniform", "normal", "zipf", "employees", "forest" })
				{
					reports.Reports[protocol].Add(distribution, new Dictionary<string, Dictionary<string, AllReports.Report>>());

					foreach (var querySize in Type != Type.QuerySizes ? new List<string> { QueryRange } : new List<double> { 0.5, 1, 1.5, 2, 3 }.Select(s => s.ToString("#.#")))
					{
						reports.Reports[protocol][distribution].Add(querySize, new Dictionary<string, AllReports.Report>());

						foreach (var dataPercent in Type != Type.DataPercent ? new List<string> { DataPercent } : new List<int> { 5, 10, 20, 50, 100 }.Select(s => s.ToString()))
						{
							AllReports.Report report = new AllReports.Report();

							dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(ReportDir, $"{protocol}-{distribution}-{querySize}-{dataPercent}-{Tail}.json")));

							foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>())
							{
								long actions = data.Report[stage.ToString()].ActionsNumber;

								report.Stages.Add(stage, new AllReports.Report.Subreport
								{
									IOs = (data.Report[stage.ToString()].IOs + actions - 1) / actions,
									CommunicationVolume = (data.Report[stage.ToString()].MessagesSent + actions - 1) / actions,
									CommunicationSize = (((data.Report[stage.ToString()].CommunicationVolume + actions - 1) / actions) + 7) / 8
								});
							}

							reports.Reports[protocol][distribution][querySize].Add(dataPercent, report);
						}
					}
				}
			}

			switch (Type)
			{
				case Type.Table:
					Table(reports);
					break;
				case Type.Plots:
					await PlotsAsync(reports);
					break;
				case Type.QuerySizes:
					await QuerySizesAsync(reports);
					break;
				case Type.DataPercent:
					await DataPercentAsync(reports);
					break;
			}

			return 0;
		}

		private void Table(AllReports reports)
		{
			string Pad(object input) => input.ToString();

			void ProcessRow(AllReports.Report report, string name)
			{
				Console.Write(Pad(name) + "\t");

				foreach (var value in new List<long>
				{
					report.Stages[Stage.Construction].IOs,
					report.Stages[Stage.Queries].IOs,
					report.Stages[Stage.Construction].CommunicationVolume,
					report.Stages[Stage.Queries].CommunicationVolume,
					report.Stages[Stage.Construction].CommunicationSize,
					report.Stages[Stage.Queries].CommunicationSize
				})
				{
					Console.Write($"& {Pad(value)}\t");
				}
				Console.WriteLine(@"\\");
			}

			AllReports.Report AverageRows(params AllReports.Report[] input)
			{
				AllReports.Report result = new AllReports.Report();
				result.Stages[Stage.Construction] = new AllReports.Report.Subreport();
				result.Stages[Stage.Queries] = new AllReports.Report.Subreport();

				foreach (var report in input)
				{
					foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>())
					{
						result.Stages[stage].IOs += report.Stages[stage].IOs;
						result.Stages[stage].CommunicationSize += report.Stages[stage].CommunicationSize;
						result.Stages[stage].CommunicationVolume += report.Stages[stage].CommunicationVolume;
					}
				}

				foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>())
				{
					result.Stages[stage].IOs /= input.Count();
					result.Stages[stage].CommunicationSize /= input.Count();
					result.Stages[stage].CommunicationVolume /= input.Count();
				}

				return result;
			}

			Console.Write(Pad("Name") + "\t");

			foreach (var value in new List<string> { "IOs", "Comm Volume", "Comm size" })
			{
				foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>())
				{
					Console.Write(Pad($"{stage.ToString()[0]} {value}" + "\t"));
				}
			}
			Console.WriteLine();

			ProcessRow(
				AverageRows(
					reports.Reports["adamore"]["employees"][QueryRange][DataPercent],
					reports.Reports["practicalore"]["employees"][QueryRange][DataPercent],
					reports.Reports["lewiore"]["employees"][QueryRange][DataPercent],
					reports.Reports["cryptdb"]["employees"][QueryRange][DataPercent],
					reports.Reports["fhope"]["employees"][QueryRange][DataPercent]
				),
				"B+ tree w. ORE"
				);
			Console.WriteLine(@"\midrule");
			ProcessRow(reports.Reports["florian"]["employees"][QueryRange][DataPercent], @"Kerschbaum~\cite{florian-protocol}");
			Console.WriteLine(@"\midrule");
			ProcessRow(reports.Reports["popecold"]["employees"][QueryRange][DataPercent], @"POPE~\cite{pope} cold");
			ProcessRow(reports.Reports["pope"]["employees"][QueryRange][DataPercent], @"POPE~\cite{pope} warm");
		}

		private async Task PlotsAsync(AllReports reports)
		{
			foreach (var value in new List<string> { "ios", "vol", "size" })
			{
				foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>().OrderBy(c => c))
				{
					using (StreamWriter file = new StreamWriter(Path.Combine(Output, $"protocols-{stage.ToString().ToLower()[0]}{value}.txt")))
					{
						foreach (var distribution in new List<string> { "uniform", "normal", "zipf", "employees", "forest" })
						{
							foreach (var protocol in new List<string> { "noencryption", "cryptdb", "practicalore", "lewiore", "fhope", "adamore", "florian", "popecold", "pope" })
							{
								long result = 0;
								switch (value)
								{
									case "ios":
										result = reports.Reports[protocol][distribution][QueryRange][DataPercent].Stages[stage].IOs;
										break;
									case "vol":
										result = reports.Reports[protocol][distribution][QueryRange][DataPercent].Stages[stage].CommunicationVolume;
										break;
									case "size":
										result = reports.Reports[protocol][distribution][QueryRange][DataPercent].Stages[stage].CommunicationSize;
										break;
								}

								await file.WriteLineAsync(result.ToString());
							}
						}
					}
				}
			}
		}

		private async Task QuerySizesAsync(AllReports reports)
		{
			foreach (var value in new List<string> { "ios", "vol", "size" })
			{
				using (StreamWriter file = new StreamWriter(Path.Combine(Output, $"protocols-query-sizes-{value}.txt")))
				{
					foreach (var querySize in new List<double> { 0.5, 1, 1.5, 2, 3 }.Select(s => s.ToString("#.#")))
					{
						foreach (var protocol in new List<string> { "noencryption", "cryptdb", "practicalore", "lewiore", "fhope", "adamore", "florian", "popecold", "pope" })
						{
							long result = 0;
							switch (value)
							{
								case "ios":
									result = reports.Reports[protocol][Distro][querySize][DataPercent].Stages[Stage.Queries].IOs;
									break;
								case "vol":
									result = reports.Reports[protocol][Distro][querySize][DataPercent].Stages[Stage.Queries].CommunicationVolume;
									break;
								case "size":
									result = reports.Reports[protocol][Distro][querySize][DataPercent].Stages[Stage.Queries].CommunicationSize;
									break;
							}

							await file.WriteLineAsync(result.ToString());
						}
					}
				}
			}
		}

		private async Task DataPercentAsync(AllReports reports)
		{
			foreach (var value in new List<string> { "ios", "vol", "size" })
			{
				foreach (var stage in Enum.GetValues(typeof(Stage)).Cast<Stage>().OrderBy(c => c))
				{
					using (StreamWriter file = new StreamWriter(Path.Combine(Output, $"protocols-data-percent-{stage.ToString().ToLower()[0]}{value}.txt")))
					{
						foreach (var dataPercent in new List<int> { 5, 10, 20, 50, 100 }.Select(s => s.ToString()))
						{
							foreach (var protocol in new List<string> { "noencryption", "cryptdb", "practicalore", "lewiore", "fhope", "adamore", "florian", "popecold", "pope" })
							{
								long result = 0;
								switch (value)
								{
									case "ios":
										result = reports.Reports[protocol][Distro][QueryRange][dataPercent].Stages[stage].IOs;
										break;
									case "vol":
										result = reports.Reports[protocol][Distro][QueryRange][dataPercent].Stages[stage].CommunicationVolume;
										break;
									case "size":
										result = reports.Reports[protocol][Distro][QueryRange][dataPercent].Stages[stage].CommunicationSize;
										break;
								}

								await file.WriteLineAsync(result.ToString());
							}
						}
					}
				}
			}
		}
	}
}
