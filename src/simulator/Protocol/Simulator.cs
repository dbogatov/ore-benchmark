using System;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Diagnostics;
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

			_cacheSize = _inputs.CacheSize;

			ClearTrackers();

			protocol.NodeVisited += new NodeVisitedEventHandler(RecordNodeVisit);
			protocol.OperationOcurred += new SchemeOperationEventHandler(RecordSchemeOperation);
			protocol.PrimitiveUsed += new PrimitiveUsageEventHandler(RecordPrimitiveUsage);
			protocol.MessageSent += new MessageSentEventHandler(RecordCommunivcationVolume);
			protocol.ClientStorage += new ClientStorageEventHandler(RecordClientStorage);

			ClearTrackers();
		}

		/// <summary>
		/// Generates a sub-report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
		private Report.SubReport Profile(Action routine, Stages stage)
		{
			var currentProcess = Process.GetCurrentProcess();

			ClearTrackers();

			var timer = System.Diagnostics.Stopwatch.StartNew();
			var processStartTime = currentProcess.UserProcessorTime;

			routine();

			var processEndTime = currentProcess.UserProcessorTime;
			timer.Stop();

			// for some reason this value is off by exactly hundred
			var procTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round((processEndTime.TotalMilliseconds - processStartTime.TotalMilliseconds) / 100));

			int actionsNumber = 0;
			switch (stage)
			{
				case Stages.Handshake:
					actionsNumber = 1;
					break;
				case Stages.Construction:
					actionsNumber = _inputs.Dataset.Count;
					break;
				case Stages.Queries:
					actionsNumber = _inputs.QueriesCount();
					break;
			}

			return new Report.SubReport
			{
				CacheSize = _inputs.CacheSize,
				CPUTime = procTime,
				ObservedTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds),
				IOs = _visited,
				AvgIOs = _visited / actionsNumber,
				SchemeOperations = _schemeOperations.Values.Sum(),
				AvgSchemeOperations = _schemeOperations.Values.Sum() / actionsNumber,
				TotalPrimitiveOperations = CloneDictionary(_primitiveUsage),
				PurePrimitiveOperations = CloneDictionary(_purePrimitiveUsage),
				MessagesSent = _rounds,
				CommunicationVolume = _communicationVolume.Item1,
				MaxClientStorage = _maxClientStorage
			};
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
}
