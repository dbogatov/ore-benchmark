using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace ColdVsWarm
{
	public class AllReports
	{
		public Dictionary<string, List<Report>> Reports = new Dictionary<string, List<Report>>();

		public class Report
		{
			public long IOs { get; set; } = 0;
			public long CommunicationVolume { get; set; } = 0;
			public long CommunicationSize { get; set; } = 0;
		}
	}

	[HelpOption("-?|-h|--help")]
	[Command(Name = "cold-warm-sim", Description = "Cold-vs-warm simulation utility", ThrowOnUnexpectedArgument = true)]
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

		[Required]
		[DirectoryExists]
		[Option("--output <DIR>", Description = "Path to the output directory for plots type.")]
		public string Output { get; }

		[Required]
		[Option("--tail <string>", Description = "Required. Tail (after protocol, distro and query-range) part of file names {protocol}-{distro}-{query-range}-{cache}-{seed}.json.")]
		public string Tail { get; }

		private async Task<int> OnExecute()
		{
			AllReports reports = new AllReports();
			int queries = 0;

			foreach (var protocol in new List<string> { "noencryption", "bclo", "clww", "lewiwu", "cloz", "fhope", "kerschbaum", "pope", "cjjjkrs", "oram" })
			{
				reports.Reports.Add(protocol, new List<AllReports.Report>());

				dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(ReportDir, $"{protocol}-{Tail}.json")));

				foreach (var subreport in data.Report.Queries.PerQuerySubreports)
				{
					reports.Reports[protocol].Add(new AllReports.Report
					{
						IOs = subreport.IOs,
						CommunicationVolume = subreport.MessagesSent,
						CommunicationSize = subreport.CommunicationVolume
					});
				}

				if (queries == 0)
				{
					queries = reports.Reports[protocol].Count;
				}
			}

			foreach (var value in new List<string> { "ios", "vol", "size" })
			{
				using (StreamWriter file = new StreamWriter(Path.Combine(Output, $"cold-vs-warm-{value}.txt")))
				{
					await file.WriteLineAsync(queries.ToString());

					foreach (var protocol in new List<string> { "noencryption", "bclo", "clww", "lewiwu", "cloz", "fhope", "kerschbaum", "pope", "cjjjkrs", "oram" })
					{
						foreach (var subreport in reports.Reports[protocol])
						{
							long result = 0;
							switch (value)
							{
								case "ios":
									result = subreport.IOs;
									break;
								case "vol":
									result = subreport.CommunicationVolume;
									break;
								case "size":
									result = subreport.CommunicationSize;
									break;
							}

							await file.WriteLineAsync(result.ToString());
						}
					}
				}
			}

			return 0;
		}
	}
}
