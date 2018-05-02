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
		private List<Tuple<int, long>> _cache;
		private long _visited = 0;
		private long _clock = 0;

		private Dictionary<SchemeOperation, long> _schemeOperations = new Dictionary<SchemeOperation, long>();
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

			_cache = new List<Tuple<int, long>>(inputs.CacheSize);
			_cache.Clear();
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
		void RecordNodeVisit(int nodeHash)
		{
			_clock++;

			if (_inputs.CacheSize > 0)
			{
				var min = Int64.MaxValue;
				var toEvict = 0;
				for (int i = 0; i < _cache.Count; i++)
				{
					var tuple = _cache[i];

					if (tuple.Item1 == nodeHash)
					{
						// cache hit
						return;
					}

					if (tuple.Item2 < min)
					{
						min = tuple.Item2;
						toEvict = i;
					}
				}

				if (_cache.Count == _inputs.CacheSize)
				{
					// Need to evict
					_cache[toEvict] = new Tuple<int, long>(nodeHash, _clock);
				}
				else
				{
					// No need to evict
					_cache.Add(new Tuple<int, long>(nodeHash, _clock));
				}

			}

			_visited++;
		}

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
				, true
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
			}, false);
		}

		/// <summary>
		/// Generates a sub-report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
		private Report.SubReport Profile(Action routine, bool constructionStage)
		{
			var currentProcess = Process.GetCurrentProcess();
			_cache.Clear();
			_schemeOperations.Keys.ToList().ForEach(key => _schemeOperations[key] = 0);
			_clock = 0;
			_visited = 0;

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
				AvgSchemeOperations = _schemeOperations.Values.Sum() / actionsNumber
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
