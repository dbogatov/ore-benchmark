using System;
using System.Collections.Generic;
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

			_protocol.RunHandshake();
			ClearTrackers();
		}

		/// <summary>
		/// Generated sub-report for the construction stage of simulation
		/// when data structure gets populated with dataset.
		/// </summary>
		private Report.SubReport ConstructionStage() =>
			Profile(() => _protocol.RunConstructionProtocol(_inputs.Dataset), constructionStage: true);

		/// <summary>
		/// Generated sub-report for the query stage of simulation
		/// when queries are run against data structure.
		/// </summary>
		private Report.SubReport QueryStage()
		{
			return Profile(() => _protocol.RunQueryProtocol(_inputs.Queries), constructionStage: false);
		}

		/// <summary>
		/// Generates a sub-report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
		private Report.SubReport Profile(Action routine, bool constructionStage)
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

			var actionsNumber = constructionStage ? _inputs.Dataset.Count : _inputs.QueriesCount();

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
			result.Stages[Stages.Construction] = ConstructionStage();
			result.Stages[Stages.Queries] = QueryStage();

			return result;
		}
	}
}
