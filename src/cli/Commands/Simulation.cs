﻿using System;
using Simulation.BPlusTree;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using DataStructures.BPlusTree;
using Simulation;

namespace CLI
{
	[HelpOption("--help")]
	public abstract class CommandBase
	{
		protected abstract int OnExecute(CommandLineApplication app);
	}

	[Command(Name = "ore-benchamark", Description = "An ORE schemes benchmark", ThrowOnUnexpectedArgument = true)]
	[VersionOptionFromMember("--version", MemberName = nameof(Version))]
	[Subcommand("tree", typeof(BPlusTreeCommand))]
	[Subcommand("scheme", typeof(PureSchemeCommand))]
	public class SimulatorCommand : CommandBase
	{
		private static string Version() => "dev";

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
