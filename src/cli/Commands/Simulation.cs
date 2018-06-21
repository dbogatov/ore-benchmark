using System;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace CLI
{
	[HelpOption("--help")]
	/// <summary>
	/// Abstract out common logic for all commands
	/// </summary>
	public abstract class CommandBase
	{
		/// <summary>
		/// Main comand's action
		/// </summary>
		/// <returns>Exit code</returns>
		protected abstract int OnExecute(CommandLineApplication app);

		/// <summary>
		/// Puts a message to console
		/// </summary>
		/// <param name="output">Message to put</param>
		/// <param name="verbose">Verbose flag (if unset, message will not be put)</param>
		protected void PutToConsole(string output, bool verbose)
		{
			if (verbose)
			{
				System.Console.WriteLine(output);
			}
		}

		/// <summary>
		/// Produces JSON string from report and extra parameters
		/// </summary>
		protected string JsonReport(object report, object parameters) =>
			JsonConvert.SerializeObject(
				new {
					Report = report,
					Parameters = parameters
				}
			);
	}

	[Command(Name = "ore-benchamark", Description = "An ORE schemes benchmark", ThrowOnUnexpectedArgument = true)]
	[VersionOptionFromMember("--version", MemberName = nameof(Version))]
	[Subcommand("protocol", typeof(ProtocolCommand))]
	[Subcommand("scheme", typeof(PureSchemeCommand))]
	public class SimulatorCommand : CommandBase
	{
		private static string Version() => GlobalVar.Version;

		[Option("--verbose|-v", "If present, more verbose output will be generated.", CommandOptionType.NoValue)]
		public bool Verbose { get; } = false;

		[Option("--seed <number>", Description = "Seed to use for all operations. Default random (depends on system time).")]
		public int Seed { get; } = new Random().Next();

		[Option("--ore-scheme <enum>", Description = "ORE scheme to use (eq. NoEncryption)")]
		public ORESchemes.Shared.ORESchemes OREScheme { get; } = ORESchemes.Shared.ORESchemes.NoEncryption;

		[Required]
		[FileExists]
		[Option("--dataset <FILE>", Description = "Required. Path to dataset file.")]
		public string Dataset { get; }

		protected override int OnExecute(CommandLineApplication app)
		{
			app.ShowHelp();

			return 1;
		}
	}
}
