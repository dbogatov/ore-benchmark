using System;
using Simulation.PureSchemes;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using Simulation;
using CLI.DataReaders;
using ORESchemes.Shared;

namespace CLI
{
	[Command(Description = "Run plain encryptions, decryptions and comparisons on a scheme")]
	public class PureSchemeCommand : CommandBase
	{
		[AllowedValues("4", "8", "16")]
		[Option("--lewi-ore-n <number>", Description = "Parameter N for LewiORE. Must be one of [4, 8, 16]. Default 16.")]
		public int LewiOREN { get; } = 16;

		[Range(32, 48)]
		[Option("--cryptdb-range <number>", Description = "Range size (in bits) for CryptDB OPE (eq. 32 gives range +/- 2^32). Must be in range [32, 48]. Default 48.")]
		public int CryptDBRange { get; } = 48;

		[Range(0, 100)]
		[Option("--fhope-p <number>", Description = "For imperfect FH-OPE, probability to generate new ciphertext. 0 means using perfect FH-OPE. Must be in range [0, 100]. Default 0.")]
		public int FHOPEP { get; } = 0;

		private SimulatorCommand Parent { get; set; }

		protected override int OnExecute(CommandLineApplication app)
		{
			PutToConsole($"Seed: {Parent.Seed}", Parent.Verbose);
			PutToConsole($"Inputs: dataset={Parent.Dataset}, scheme={Parent.OREScheme}", Parent.Verbose);

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new PureScheme(Parent.Dataset);

			timer.Stop();

			PutToConsole($"Dataset of {reader.Dataset.Count} records.", Parent.Verbose);
			PutToConsole($"Inputs read in {timer.ElapsedMilliseconds} ms.", Parent.Verbose);

			AbsReport<Stages> report;

			switch (Parent.OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					report =
						new Simulator<OPECipher, BytesKey>(
							reader.Dataset,
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					PutToConsole($"CryptDB range is [{Convert.ToInt64(-Math.Pow(2, CryptDBRange))}, {Convert.ToInt64(-Math.Pow(2, CryptDBRange))}]", Parent.Verbose);
					report =
						new Simulator<OPECipher, BytesKey>(
							reader.Dataset,
							new CryptDBOPEFactory(Parent.Seed).GetScheme(CryptDBRange)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					report =
						new Simulator<ORESchemes.PracticalORE.Ciphertext, BytesKey>(
							reader.Dataset,
							new PracticalOREFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					PutToConsole($"LewiORE N = {LewiOREN}", Parent.Verbose);
					report =
						new Simulator<ORESchemes.LewiORE.Ciphertext, ORESchemes.LewiORE.Key>(
							reader.Dataset,
							new LewiOREFactory(Parent.Seed).GetScheme(LewiOREN)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.FHOPE:
					PutToConsole($"FH-OPE p = {FHOPEP}", Parent.Verbose);
					report =
						new Simulator<ORESchemes.FHOPE.Ciphertext, ORESchemes.FHOPE.State>(
							reader.Dataset,
							new FHOPEFactory(Parent.Seed).GetScheme(FHOPEP)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.AdamORE:
					report =
						new Simulator<ORESchemes.AdamORE.Ciphertext, ORESchemes.AdamORE.Key>(
							reader.Dataset,
							new AdamOREFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				default:
					throw new NotImplementedException($"Scheme {Parent.OREScheme} is not yet supported");
			}

			System.Console.WriteLine(
				Parent.Verbose ? 
				report.ToString() : 
				JsonReport(
					report.Stages,
					new {
						CryptDBRange = CryptDBRange,
						FHOPEP = FHOPEP,
						LewiOREN = LewiOREN,
						Dataset = Parent.Dataset,
						OREScheme = Parent.OREScheme,
						Seed = Parent.Seed
					}
				)
			);

			return 0;
		}
	}
}
