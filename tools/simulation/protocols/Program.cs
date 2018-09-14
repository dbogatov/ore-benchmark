using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Schemes
{
	enum Type
	{
		Table, Plots
	}

	public enum Stage
	{
		Construction, Queries
	}

	public class AllReports
	{
		public Dictionary<string, Dictionary<string, Report>> Reports = new Dictionary<string, Dictionary<string, Report>>();

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
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Required]
		[DirectoryExists]
		[Option("--data <DIR>", Description = "Required. Path to the directory with protocol output files in JSON.")]
		public string ReportDir { get; }

		[Required]
		[Option("--tail <string>", Description = "Required. Tail (after protocol and distro) part of file names {protocol}-{distro}-{query-range}-{cache}-{seed}.json.")]
		public string Tail { get; }

		[Required]
		[Option("--type <enum>", Description = "Required. Type of output - table or plots.")]
		public Type Type { get; }

		private int OnExecute()
		{
			AllReports reports = new AllReports();

			foreach (var protocol in new List<string> { "adamore", "cryptdb", "practicalore", "lewiore", "fhope", "florian", "pope", "noencryption", "popecold" })
			{
				reports.Reports.Add(protocol, new Dictionary<string, AllReports.Report>());

				foreach (var distribution in new List<string> { "uniform", "normal", "zipf", "employees", "forest" })
				{
					AllReports.Report report = new AllReports.Report();

					dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(ReportDir, $"{protocol}-{distribution}-{Tail}.json")));

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

					reports.Reports[protocol].Add(distribution, report);
				}
			}

			switch (Type)
			{
				case Type.Table:
					Table(reports);
					break;
				case Type.Plots:
					Plots(reports);
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
					reports.Reports["adamore"]["employees"],
					reports.Reports["practicalore"]["employees"],
					reports.Reports["lewiore"]["employees"],
					reports.Reports["cryptdb"]["employees"],
					reports.Reports["fhope"]["employees"]
				),
				"B+ tree w. ORE"
				);
			Console.WriteLine(@"\midrule");
			ProcessRow(reports.Reports["florian"]["employees"], @"Kerschbaum~\cite{florian-protocol}");
			Console.WriteLine(@"\midrule");
			ProcessRow(reports.Reports["popecold"]["employees"], @"POPE~\cite{pope} cold");
			ProcessRow(reports.Reports["pope"]["employees"], @"POPE~\cite{pope} warm");
		}

		private void Plots(AllReports reports)
		{

		}
	}
}
