using System;
using Simulation.PureSchemes;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using Simulation;
using CLI.DataReaders;
using Crypto.Shared;
using System.Linq;

namespace CLI
{
	[Command(Description = "Run plain encryptions, decryptions and comparisons on a scheme")]
	public class PureSchemeCommand : CommandBase
	{
		[AllowedValues("4", "8", "16")]
		[Option("--lewi-wu-n <number>", Description = "Parameter N for LewiWu. Must be one of [4, 8, 16]. Default 16.")]
		public int LewiWuN { get; } = 16;

		[Range(32, 48)]
		[Option("--bclo-range <number>", Description = "Range size (in bits) for BCLO OPE (e.g. 32 gives range +/- 2^32). Must be in range [32, 48]. Default 48.")]
		public int BCLORange { get; } = 48;

		[Range(0, 100)]
		[Option("--fhope-p <number>", Description = "For imperfect FH-OPE, probability to generate new ciphertext. 0 means using perfect FH-OPE. Must be in range [0, 100]. Default 0.")]
		public int FHOPEP { get; } = 0;

		public SimulatorCommand Parent { get; set; }

		protected override int OnExecute(CommandLineApplication app)
		{
			Simulate();

			return 0;
		}

		public AbsReport<Stages> Simulate()
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, scheme={Parent.Protocol}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new PureScheme(Parent.Dataset);

			timer.Stop();

			PutToConsole($"Dataset of {reader.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			AbsReport<Stages> report;

			switch (Parent.Protocol)
			{
				case Crypto.Shared.Protocols.NoEncryption:
					report =
						new Simulator<OPECipher, BytesKey>(
							reader.Dataset,
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case Crypto.Shared.Protocols.BCLO:
					PutToConsole($"BCLO range is [{Convert.ToInt64(-Math.Pow(2, BCLORange))}, {Convert.ToInt64(Math.Pow(2, BCLORange))}]", Parent.Verbose);
					report =
						new Simulator<OPECipher, BytesKey>(
							reader.Dataset,
							new BCLOFactory(Parent.Seed).GetScheme(BCLORange)
						).Simulate();
					break;
				case Crypto.Shared.Protocols.CLWW:
					report =
						new Simulator<Crypto.CLWW.Ciphertext, BytesKey>(
							reader.Dataset,
							new CLWWFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case Crypto.Shared.Protocols.LewiWu:
					PutToConsole($"LewiWu N = {LewiWuN}", Parent.Verbose);
					report =
						new Simulator<Crypto.LewiWu.Ciphertext, Crypto.LewiWu.Key>(
							reader.Dataset,
							new LewiWuFactory(Parent.Seed).GetScheme(LewiWuN)
						).Simulate();
					break;
				case Crypto.Shared.Protocols.FHOPE:
					PutToConsole($"FH-OPE p = {FHOPEP}", Parent.Verbose);
					report =
						new Simulator<Crypto.FHOPE.Ciphertext, Crypto.FHOPE.State>(
							reader.Dataset,
							new FHOPEFactory(Parent.Seed).GetScheme(FHOPEP)
						).Simulate();
					break;
				case Crypto.Shared.Protocols.CLOZ:
					report =
						new Simulator<Crypto.CLOZ.Ciphertext, Crypto.CLOZ.Key>(
							reader.Dataset,
							new CLOZFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				default:
					throw new NotImplementedException($"Protocol {Parent.Protocol} is not yet supported");
			}

			if (!Parent.Extended)
			{
				report.Stages.Values.ToList().ForEach(s => s.PerQuerySubreports.Clear());
			}

			if (!Parent.ForScript)
			{
				System.Console.WriteLine(
					Parent.Verbose && !Parent.Extended ?
					report.ToString() :
					JsonReport(
						report.Stages,
						new
						{
							BCLORange = BCLORange,
							FHOPEP = FHOPEP,
							LewiWuN = LewiWuN,
							Dataset = Parent.Dataset,
							OREScheme = Parent.Protocol,
							Seed = Parent.Seed
						}
					)
				);
			}

			return report;
		}
	}
}
