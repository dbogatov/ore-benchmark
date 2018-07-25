using System;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Linq;
using ORESchemes.Shared.Primitives;

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

			perQuery = new Tracker(_inputs.CacheSize);
			perStage = new Tracker(_inputs.CacheSize);

			protocol.NodeVisited += RecordNodeVisit;
			protocol.OperationOcurred += RecordSchemeOperation;
			protocol.PrimitiveUsed +=  RecordPrimitiveUsage;
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
