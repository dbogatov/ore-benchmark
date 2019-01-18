using System;
using System.Linq;
using BPlusTree;
using Crypto.Shared;

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

			perQuery = new Tracker(_inputs.CacheSize, _inputs.CachePolicy);
			perStage = new Tracker(_inputs.CacheSize, _inputs.CachePolicy);

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
		/// <param name="elementsPerPage">Option that controls the number of elements per page for I/Os (branching factor for B+ tree)</param>
		/// <returns>An instantiated protocol</returns>
		public static IProtocol GenerateProtocol(Crypto.Shared.Protocols scheme, int seed, int elementsPerPage)
		{
			switch (scheme)
			{
				case Crypto.Shared.Protocols.NoEncryption:
					return
						new Simulation.Protocol.SimpleORE.Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new NoEncryptionFactory().GetScheme(),
								elementsPerPage
							),
							new NoEncryptionFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.BCLO:
					return
						new Simulation.Protocol.SimpleORE.Protocol<Crypto.BCLO.Scheme, OPECipher, BytesKey>(
							new Options<OPECipher>(
								new BCLOFactory().GetScheme(),
								elementsPerPage
							),
							new BCLOFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.CLWW:
					return
						new Simulation.Protocol.SimpleORE.Protocol<Crypto.CLWW.Scheme, Crypto.CLWW.Ciphertext, BytesKey>(
							new Options<Crypto.CLWW.Ciphertext>(
								new CLWWFactory().GetScheme(),
								elementsPerPage
							),
							new CLWWFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.LewiWu:
					return
						new Simulation.Protocol.LewiWu.Protocol(
							new Options<Crypto.LewiWu.Ciphertext>(
								new LewiWuFactory().GetScheme(),
								elementsPerPage
							),
							new LewiWuFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.FHOPE:
					return
						new Simulation.Protocol.FHOPE.Protocol(
							new Options<Crypto.FHOPE.Ciphertext>(
								new FHOPEFactory().GetScheme(),
								elementsPerPage
							),
							new FHOPEFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.CLOZ:
					return
						new Simulation.Protocol.SimpleORE.Protocol<Crypto.CLOZ.Scheme, Crypto.CLOZ.Ciphertext, Crypto.CLOZ.Key>(
							new Options<Crypto.CLOZ.Ciphertext>(
								new CLOZFactory().GetScheme(),
								elementsPerPage
							),
							new CLOZFactory(seed).GetScheme()
						);
				case Crypto.Shared.Protocols.Kerschbaum:
					return
						new Simulation.Protocol.Kerschbaum.Protocol(
							new Random(seed).GetBytes(128 / 8),
							elementsPerPage
						);
				case Crypto.Shared.Protocols.POPE:
					return
						new Simulation.Protocol.POPE.Protocol(
							new Random(seed).GetBytes(128 / 8),
							elementsPerPage
						);
				case Crypto.Shared.Protocols.ORAM:
					return
						new Simulation.Protocol.ORAM.Protocol(
							new Random(seed).GetBytes(128 / 8),
							elementsPerPage,
							128
						);
				case Crypto.Shared.Protocols.CJJKRS:
					return
						new Simulation.Protocol.SSE.CJJKRS.Protocol(
							new Random(seed).GetBytes(128 / 8),
							elementsPerPage
						);
				case Crypto.Shared.Protocols.CJJJKRS:
					return
						new Simulation.Protocol.SSE.CJJJKRS.Protocol(
							new Random(seed).GetBytes(128 / 8),
							elementsPerPage
						);
				default:
					throw new NotImplementedException($"Protocol {scheme} is not yet supported");
			}
		}
	}

	public class Tracker : AbsTracker
	{
		public Tracker(int cacheSize, CachePolicy cachePolicy)
		{
			_cacheSize = cacheSize;
			_cachePolicy = cachePolicy;
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
