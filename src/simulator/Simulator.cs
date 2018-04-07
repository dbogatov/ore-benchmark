using System;
using System.Collections.Generic;
using OPESchemes;
using DataStructures.BPlusTree;
using System.Diagnostics;
using System.Linq;

namespace Simulation
{
	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class Simulator<I, D>
	{
		private HashSet<int> _visited = new HashSet<int>();
		private Dictionary<SchemeOperation, int> _schemeOperations = new Dictionary<SchemeOperation, int>();
		private Inputs<I, D> _inputs;
		private IOPEScheme<I, I> _scheme;
		private int _key;
		private Tree<D, I, I> _tree;

		public Simulator(Inputs<I, D> inputs, Options<I, I> options)
		{
			_inputs = inputs;
			_scheme = options.Scheme;
			_key = options.Scheme.KeyGen();

			options.NodeVisited += new NodeVisitedEventHandler(RecordNodeVisit);
			options.Scheme.OperationOcurred += new SchemeOperationEventHandler(RecordSchemeOperation);

			_visited.Clear();
			Enum
				.GetValues(typeof(SchemeOperation))
				.OfType<SchemeOperation>()
				.ToList()
				.ForEach(val => _schemeOperations.Add(val, 0));

			_tree = new Tree<D, I, I>(options);
		}

		void RecordNodeVisit(int nodeHash) => _visited.Add(nodeHash);
		void RecordSchemeOperation(SchemeOperation operation) => _schemeOperations[operation]++;

		private Report.SubReport ConstructionStage() =>
			Profile(() =>
				_inputs
					.Dataset
					.ForEach(record => _tree.Insert(_scheme.Encrypt(record.index, _key), record.value))
			);

		private Report.SubReport QueryStage()
		{
			return Profile(() =>
			{
				switch (_inputs.Type)
				{
					case QueriesType.Exact:
						_inputs
							.ExactQueries
							.ForEach(query => _tree.TryGet(_scheme.Encrypt(query.index, _key), out _));
						break;
					case QueriesType.Range:
						_inputs
							.RangeQueries
							.ForEach(
								query =>
									_tree
										.TryRange(
											_scheme.Encrypt(query.from, _key),
											_scheme.Encrypt(query.to, _key),
											 out _
											)
									);
						break;
					case QueriesType.Update:
						_inputs
							.UpdateQueries
							.ForEach(query => _tree.Insert(_scheme.Encrypt(query.index, _key), query.value));
						break;
					case QueriesType.Delete:
						_inputs
							.DeleteQueries
							.ForEach(query => _tree.Delete(_scheme.Encrypt(query.index, _key)));
						break;
					default:
						break;
				}
			});
		}

		private Report.SubReport Profile(Action routine)
		{
			var currentProcess = Process.GetCurrentProcess();
			_visited.Clear();
			_schemeOperations.Keys.ToList().ForEach(key => _schemeOperations[key] = 0);

			var timer = System.Diagnostics.Stopwatch.StartNew();
			var processStartTime = currentProcess.UserProcessorTime;

			routine();

			var processEndTime = currentProcess.UserProcessorTime;
			timer.Stop();

			return new Report.SubReport
			{
				CPUTime = processEndTime - processStartTime,
				ObservedTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds),
				IOs = _visited.Count,
				SchemeOperations = _schemeOperations.Values.Sum()
			};
		}

		public Report Simulate()
		{
			return new Report
			{
				Construction = ConstructionStage(),
				Queries = QueryStage()
			};
		}
	}
}
