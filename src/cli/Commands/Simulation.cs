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
				new
				{
					Report = report,
					Parameters = parameters,
					Version = Version.Value()
				}
			);
	}

	[Command(Name = "ore-benchamark", Description = "An ORE schemes benchmark", ThrowOnUnexpectedArgument = true)]
	[VersionOptionFromMember("--version", MemberName = nameof(Version))]
	[Subcommand("protocol", typeof(ProtocolCommand))]
	[Subcommand("scheme", typeof(PureSchemeCommand))]
	public class SimulatorCommand : CommandBase
	{
		private static string Version() => CLI.Version.Value();

		[Option("--verbose|-v", "If present, report summary will be output instead of JSON.", CommandOptionType.NoValue)]
		public bool Verbose { get; set; } = false;

		[Option("--extended|-V", "If present, JSON will contain per-query subreports. Overrides -v flag. WARNING: use with care, files can easily grow gigabytes.", CommandOptionType.NoValue)]
		public bool Extended { get; set; } = false;

		[Option("--for-script|-S", "If present, nothing will be put to console. Use when running the simulation from within another program.", CommandOptionType.NoValue)]
		public bool ForScript { get; set; } = false;

		[Option("--seed <number>", Description = "Seed to use for all operations. Default random (depends on system time).")]
		public int Seed { get; set; } = new Random().Next();

		[Option("--protocol <enum>", Description = "Protocol / scheme to use. Default NoEncryption.")]
		public Crypto.Shared.Protocols Protocol { get; set; } = Crypto.Shared.Protocols.NoEncryption;

		[Required]
		[FileExists]
		[Option("--dataset <FILE>", Description = "Required. Path to dataset file.")]
		public string Dataset { get; set; }

		protected override int OnExecute(CommandLineApplication app)
		{
			app.ShowHelp();

			return 1;
		}
	}

	/// <summary>
	/// Class responsible for version string
	/// </summary>
	public class AbsVersion
	{
		public override string ToString() => "local-dev";
	}

	/// <summary>
	/// If CI generates a version string, it produces a file with
	/// part of this class
	/// </summary>
	public partial class Version : AbsVersion
	{
		/// <summary>
		/// Version string value
		/// </summary>
		public static string Value() => new Version().ToString();
	}
}
