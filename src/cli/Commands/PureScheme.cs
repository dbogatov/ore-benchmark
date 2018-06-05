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
		private SimulatorCommand Parent { get; set; }

		protected override int OnExecute(CommandLineApplication app)
		{
			Console.WriteLine($"Seed: {Parent.Seed}");
			Console.WriteLine($"Inputs: dataset={Parent.Dataset}, scheme={Parent.OREScheme}");

			var timer = System.Diagnostics.Stopwatch.StartNew();

			var reader = new PureScheme(Parent.Dataset);

			timer.Stop();

			Console.WriteLine($"Dataset of {reader.Dataset.Count} records.");
			Console.WriteLine($"Inputs read in {timer.ElapsedMilliseconds} ms.");

			Report report;

			switch (Parent.OREScheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					report =
						new Simulator<long>(
							reader.Dataset,
							new NoEncryptionFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					report =
						new Simulator<long>(
							reader.Dataset,
							new CryptDBOPEFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					report =
						new Simulator<ORESchemes.PracticalORE.Ciphertext>(
							reader.Dataset,
							new PracticalOREFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					report =
						new Simulator<ORESchemes.LewiORE.Ciphertext>(
							reader.Dataset,
							new LewiOREFactory(Parent.Seed).GetScheme()
						).Simulate();
					break;
				default:
					throw new InvalidOperationException($"No such scheme: {Parent.OREScheme}");
			}

			System.Console.WriteLine(Parent.Verbose ? report.ToString() : report.ToConciseString());

			return 0;
		}
	}
}
