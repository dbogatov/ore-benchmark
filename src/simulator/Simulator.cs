using System;
using System.Collections.Generic;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Diagnostics;
using System.Linq;

namespace Simulation
{
	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// C - ciphertext type
	/// </summary>
	public class Simulator<I, D, C>
	{
		private HashSet<int> _visited = new HashSet<int>();
		private Dictionary<SchemeOperation, int> _schemeOperations = new Dictionary<SchemeOperation, int>();
		private Inputs<I, D> _inputs;
		private IOREScheme<I, C> _scheme;
		private byte[] _key;
		private Tree<D, C, I> _tree;

		public Simulator(Inputs<I, D> inputs, Options<I, C> options)
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

			_tree = new Tree<D, C, I>(options);
		}

		/// <summary>
		/// Handler for the event that node has been visited
		/// </summary>
		/// <param name="nodeHash">hash of the node</param>
		void RecordNodeVisit(int nodeHash) => _visited.Add(nodeHash);

		/// <summary>
		/// Handler for the event that the scheme has performed an operation
		/// </summary>
		/// <param name="operation">Performed operation</param>
		void RecordSchemeOperation(SchemeOperation operation) => _schemeOperations[operation]++;

		/// <summary>
		/// Generated sub-report for the construction stage of simulation
		/// when data structure gets populated with dataset.
		/// </summary>
		private Report.SubReport ConstructionStage() =>
			Profile(() =>
				_inputs
					.Dataset
					.ForEach(record => _tree.Insert(_scheme.Encrypt(record.index, _key), record.value))
			);

		/// <summary>
		/// Generated sub-report for the query stage of simulation
		/// when queries are run against data structure.
		/// </summary>
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

		/// <summary>
		/// Generates a sub-report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
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

		/// <summary>
		/// Runs simulation for the inputs and options provided through
		/// the constructor.
		/// </summary>
		/// <returns>A final report containing sub-reports for different stages</returns>
		public Report Simulate()
		{
			return new Report
			{
				QueriesType = _inputs.Type,
				Construction = ConstructionStage(),
				Queries = QueryStage()
			};
		}
	}
}
