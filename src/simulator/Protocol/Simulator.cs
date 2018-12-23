using System;
using System.Linq;
using BPlusTree;
using ORESchemes.AdamORE;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;

namespace Simulation.Protocol
{
	public class Simulator : AbsSimulator<Stages>
	{
		// Protocol and inputs
		private Inputs _inputs;
		private IProtocol _protocol;

		public Simulator(Inputs inputs, IProtocol protocol)
		{
			_inputs = inputs;
			_protocol = protocol;

			if (_inputs.CacheSize < 0)
			{
				throw new ArgumentException($"Cache size must not be negative. Given {_inputs.CacheSize}.");
			}

			perQuery = new Tracker(_inputs.CacheSize);
			perStage = new Tracker(_inputs.CacheSize);

			protocol.NodeVisited += RecordNodeVisit;
			protocol.OperationOcurred += RecordSchemeOperation;
			protocol.PrimitiveUsed += RecordPrimitiveUsage;
			protocol.MessageSent += RecordCommunivcationVolume;
			protocol.ClientStorage += RecordClientStorage;

			protocol.Timer += TimerHandler;

			protocol.QueryCompleted += QueryReport;
		}

		/// <summary>
		/// Generates a sub-report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
		private Report.SubReport Profile(Action routine, Stages stage)
		{
			TimerHandler(stop: false);

			routine();

			TimerHandler(stop: true);

			var report = (Report.SubReport)StageReport();

			switch (stage)
			{
				case Stages.Handshake:
					report.ActionsNumber = 1;
					break;
				case Stages.Construction:
					report.ActionsNumber = _inputs.Dataset.Count;
					break;
				case Stages.Queries:
					report.ActionsNumber = _inputs.QueriesCount();
					break;
			}

			return report;
		}

		public override AbsReport<Stages> Simulate()
		{
			var result = new Report();
			result.Stages[Stages.Handshake] = Profile(() => _protocol.RunHandshake(), stage: Stages.Handshake);
			result.Stages[Stages.Construction] = Profile(() => _protocol.RunConstructionProtocol(_inputs.Dataset), stage: Stages.Construction);
			result.Stages[Stages.Queries] = Profile(() => _protocol.RunQueryProtocol(_inputs.Queries), stage: Stages.Queries);

			return result;
		}

		/// <summary>
		/// Generate a protocol of supplied arguments
		/// </summary>
		/// <param name="scheme">The scheme / protocol to use</param>
		/// <param name="seed">Seed value to use (if supplied, deterministic</param>
		/// <param name="branches">Option that controls the number of elements per page for I/Os (branching factor for B+ tree)</param>
		/// <returns>An instantiated protocol</returns>
		public static IProtocol GenerateProtocol(ORESchemes.Shared.ORESchemes scheme, int seed, int branches)
		{
			switch (scheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					return
						new Simulation.Protocol.SimpleORE.Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new NoEncryptionFactory().GetScheme(),
								branches
							),
							new NoEncryptionFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.CryptDB:
					return
						new Simulation.Protocol.SimpleORE.Protocol<CryptDBScheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new CryptDBOPEFactory().GetScheme(),
								branches
							),
							new CryptDBOPEFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					return
						new Simulation.Protocol.SimpleORE.Protocol<PracticalOREScheme, ORESchemes.PracticalORE.Ciphertext, BytesKey>(
							new Options<ORESchemes.PracticalORE.Ciphertext>(
								new PracticalOREFactory().GetScheme(),
								branches
							),
							new PracticalOREFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.LewiORE:
					return
						new Simulation.Protocol.LewiORE.Protocol(
							new Options<ORESchemes.LewiORE.Ciphertext>(
								new LewiOREFactory().GetScheme(),
								branches
							),
							new LewiOREFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.FHOPE:
					return
						new Simulation.Protocol.FHOPE.Protocol(
							new Options<ORESchemes.FHOPE.Ciphertext>(
								new FHOPEFactory().GetScheme(),
								branches
							),
							new FHOPEFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.AdamORE:
					return
						new Simulation.Protocol.SimpleORE.Protocol<AdamOREScheme, ORESchemes.AdamORE.Ciphertext, ORESchemes.AdamORE.Key>(
							new Options<ORESchemes.AdamORE.Ciphertext>(
								new AdamOREFactory().GetScheme(),
								branches
							),
							new AdamOREFactory(seed).GetScheme()
						);
				case ORESchemes.Shared.ORESchemes.Florian:
					return
						new Simulation.Protocol.Florian.Protocol(
							new Random(seed).GetBytes(128 / 8),
							branches
						);
				case ORESchemes.Shared.ORESchemes.POPE:
					return
						new Simulation.Protocol.POPE.Protocol(
							new Random(seed).GetBytes(128 / 8),
							branches
						);
				default:
					throw new NotImplementedException($"Scheme {scheme} is not yet supported");
			}
		}
	}

	public class Tracker : AbsTracker
	{
		public Tracker(int cacheSize)
		{
			_cacheSize = cacheSize;
		}

		public override AbsSubReport ReadMetrics()
		{
			return new Protocol.Report.SubReport
			{
				CacheSize = _cacheSize,
				ObservedTime = _totalTime,
				IOs = _visited,
				SchemeOperations = _schemeOperations.Values.Sum(),
				TotalPrimitiveOperations = CloneDictionary(_primitiveUsage),
				PurePrimitiveOperations = CloneDictionary(_purePrimitiveUsage),
				MessagesSent = _rounds,
				CommunicationVolume = _communicationVolume.Item1,
				MaxClientStorage = _maxClientStorage
			};
		}
	}
}
