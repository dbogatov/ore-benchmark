using System;
using System.Collections.Generic;
using OPESchemes;
using DataStructures.BPlusTree;

namespace Simulation
{
	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class Simulator<I, D>
	{
		private HashSet<int> _visited = new HashSet<int>();
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
			_tree = new Tree<D, I, I>(options);
		}

		void RecordNodeVisit(int nodeHash) => _visited.Add(nodeHash);

		private Report.SubReport ConstructionStage()
		{
			_visited.Clear();

			var timer = System.Diagnostics.Stopwatch.StartNew();

			_inputs
				.Dataset
				.ForEach(record => _tree.Insert(_scheme.Encrypt(record.index, _key), record.value));

			timer.Stop();

			return new Report.SubReport
			{
				IOs = _visited.Count,
				Time = (int)timer.ElapsedMilliseconds
			};
		}

		private Report.SubReport QueryStage()
		{
			_visited.Clear();

			var timer = System.Diagnostics.Stopwatch.StartNew();

			switch (_inputs.Type)
			{
				case QueriesType.Exact:
					_inputs
						.ExactQueries
						.ForEach(query => _tree.TryGet(_scheme.Encrypt(query.index, _key), out _));
					break;
				default:
					break;
			}

			timer.Stop();

			return new Report.SubReport
			{
				IOs = _visited.Count,
				Time = (int)timer.ElapsedMilliseconds
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
