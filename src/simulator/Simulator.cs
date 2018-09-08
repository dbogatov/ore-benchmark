using System;
using System.Collections.Generic;
using ORESchemes.Shared;
using System.Linq;
using ORESchemes.Shared.Primitives;
using System.Diagnostics;

namespace Simulation
{
	/// <summary>
	/// Undocumented methods mirror corresponding Tracker's methods
	/// </summary>
	/// <typeparam name="S">Stages enum</typeparam>
	public abstract class AbsSimulator<S> where S : Enum
	{
		protected AbsTracker perStage;
		protected AbsTracker perQuery;

		private List<AbsSubReport> perQueryReports = new List<AbsSubReport>();

		/// <summary>
		/// Generate per query report and clear corresponding tracker
		/// </summary>
		protected void QueryReport()
		{
			perQueryReports.Add(perQuery.ReadMetrics());
			perQuery.ClearTrackers();
		}

		/// <summary>
		/// Generate stage report that includes query reports
		/// </summary>
		protected AbsSubReport StageReport()
		{
			var result = perStage.ReadMetrics();
			result.PerQuerySubreports = perQueryReports;
			perStage.ClearTrackers();

			return result;
		}

		public void RecordNodeVisit(int nodeHash)
		{
			perQuery.RecordNodeVisit(nodeHash);
			perStage.RecordNodeVisit(nodeHash);
		}

		public void RecordSchemeOperation(SchemeOperation operation)
		{
			perQuery.RecordSchemeOperation(operation);
			perStage.RecordSchemeOperation(operation);
		}

		public void RecordPrimitiveUsage(Primitive primitive, bool impure)
		{
			perQuery.RecordPrimitiveUsage(primitive, impure);
			perStage.RecordPrimitiveUsage(primitive, impure);
		}

		public void RecordCommunivcationVolume(long size)
		{
			perQuery.RecordCommunivcationVolume(size);
			perStage.RecordCommunivcationVolume(size);
		}

		public void RecordClientStorage(long size)
		{
			perQuery.RecordClientStorage(size);
			perStage.RecordClientStorage(size);
		}

		public void TimerHandler(bool stop)
		{
			perQuery.TimerHandler(stop);
			perStage.TimerHandler(stop);
		}

		public void RecordMaxCipherStateSize(bool isState, long size)
		{
			perQuery.RecordMaxCipherStateSize(isState, size);
			perStage.RecordMaxCipherStateSize(isState, size);
		}

		/// <summary>
		/// Runs simulation for the inputs and options provided through
		/// the constructor.
		/// </summary>
		/// <returns>A final report containing sub-reports for different stages</returns>
		public abstract AbsReport<S> Simulate();
	}

	public abstract class AbsTracker
	{
		// Cache structures
		protected List<Tuple<int, long>> _cache;
		protected int _cacheSize = 0;
		protected long _visited = 0;
		protected long _clock = 0;

		// Scheme operations structures
		protected Dictionary<SchemeOperation, long> _schemeOperations = new Dictionary<SchemeOperation, long>();

		// Primitive usage structures
		protected Dictionary<Primitive, long> _primitiveUsage = new Dictionary<Primitive, long>();
		protected Dictionary<Primitive, long> _purePrimitiveUsage = new Dictionary<Primitive, long>();

		// Communication volume structures
		protected Tuple<long, long> _communicationVolume = new Tuple<long, long>(0, 0);
		protected long _rounds = 0;

		// Client storage structures
		protected long _maxClientStorage = 0;

		// Timer structures
		protected TimeSpan _totalTime = TimeSpan.Zero;
		protected Stopwatch _timer = new Stopwatch();

		// Cipher and state size structures
		protected long _maxCipherSize = 0;
		protected long _maxStateSize = 0;

		public AbsTracker() => ClearTrackers();

		/// <summary>
		/// Zero all trackers
		/// </summary>
		public void ClearTrackers()
		{
			_visited = 0;
			_cache = new List<Tuple<int, long>>(_cacheSize);
			_cache.Clear();

			_schemeOperations.Clear();
			Enum
				.GetValues(typeof(SchemeOperation))
				.OfType<SchemeOperation>()
				.ToList()
				.ForEach(val => _schemeOperations.Add(val, 0));

			_primitiveUsage.Clear();
			_purePrimitiveUsage.Clear();
			Enum
				.GetValues(typeof(Primitive))
				.OfType<Primitive>()
				.ToList()
				.ForEach(val =>
				{
					_primitiveUsage.Add(val, 0);
					_purePrimitiveUsage.Add(val, 0);
				});

			_communicationVolume = new Tuple<long, long>(0, 0);
			_rounds = 0;

			_maxClientStorage = 0;

			_totalTime = TimeSpan.Zero;
			_timer = new Stopwatch();

			_maxCipherSize = 0;
			_maxStateSize = 0;
		}

		/// <summary>
		/// Handler for the event that node has been visited
		/// </summary>
		/// <param name="nodeHash">hash of the node</param>
		public void RecordNodeVisit(int nodeHash)
		{
			_clock++;

			if (_cacheSize > 0)
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

				if (_cache.Count == _cacheSize)
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
		public void RecordSchemeOperation(SchemeOperation operation) => _schemeOperations[operation]++;

		/// <summary>
		/// Handler for the event that the primitive has been used
		/// </summary>
		/// <param name="operation">Primitive used</param>
		public void RecordPrimitiveUsage(Primitive primitive, bool impure)
		{
			_primitiveUsage[primitive]++;
			if (!impure)
			{
				_purePrimitiveUsage[primitive]++;
			}
		}

		/// <summary>
		/// Handler for the event that message of certain size was sent
		/// </summary>
		/// <param name="size">Size of the message</param>
		public void RecordCommunivcationVolume(long size)
		{
			_communicationVolume = new Tuple<long, long>(
				_communicationVolume.Item1 + size,
				Math.Max(_communicationVolume.Item2, size)
			);
			_rounds++;
		}

		/// <summary>
		/// Handler for the event that client storage changed
		/// </summary>
		/// <param name="size">Current value of client storage</param>
		public void RecordClientStorage(long size) => _maxClientStorage = Math.Max(_maxClientStorage, size);

		/// <summary>
		/// Handler for the event when timer needs to be stopped or resumed
		/// </summary>
		/// <param name="stop">True if timer needs to be paused, true if timer needs to be resumed</param>
		public void TimerHandler(bool stop)
		{
			if (stop)
			{
				if (_timer.IsRunning)
				{
					_timer.Stop();
					_totalTime += _timer.Elapsed;
				}
			}
			else
			{
				if (!_timer.IsRunning)
				{
					_timer.Start();
				}
			}
		}

		public void RecordMaxCipherStateSize(bool isState, long size)
		{
			if (isState)
			{
				_maxStateSize = Math.Max(_maxStateSize, size);
			}
			else
			{
				_maxCipherSize = Math.Max(_maxCipherSize, size);
			}
		}

		/// <summary>
		/// Produce a deep (not shallow) copy of its argument
		/// </summary>
		/// <param name="original">Dictionary to copy</param>
		protected Dictionary<Primitive, long> CloneDictionary(Dictionary<Primitive, long> original)
		{
			Dictionary<Primitive, long> copy = new Dictionary<Primitive, long>();

			original.Keys.ToList().ForEach(key => copy.Add(key, original[key]));

			return copy;
		}

		/// <summary>
		/// Generate report from current (as-of-now) data
		/// </summary>
		public abstract AbsSubReport ReadMetrics();
	}
}
