using System;
using Simulation.PureSchemes;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using Simulation;
using CLI.DataReaders;

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
						new Simulator<long, object>(
							reader.Dataset,
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					PutToConsole($"CryptDB range is [{Convert.ToInt64(-Math.Pow(2, CryptDBRange))}, {Convert.ToInt64(-Math.Pow(2, CryptDBRange))}]", Parent.Verbose);
					report =
						new Simulator<long, byte[]>(
							reader.Dataset,
							new CryptDBOPEFactory(Parent.Seed).GetScheme(CryptDBRange)
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					report =
						new Simulator<ORESchemes.PracticalORE.Ciphertext, byte[]>(
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
					report =
						new Simulator<ORESchemes.FHOPE.Ciphertext, ORESchemes.FHOPE.State>(
							reader.Dataset,
							new FHOPEFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				default:
					throw new NotImplementedException($"Scheme {Parent.OREScheme} is not yet supported");
			}

			if (!Parent.Verbose)
			{
				System.Console.Write($"{Parent.Seed},{LewiOREN},{CryptDBRange},{Parent.OREScheme},");
			}

			System.Console.WriteLine(Parent.Verbose ? report.ToString() : report.ToConciseString());

			return 0;
		}
	}
}
