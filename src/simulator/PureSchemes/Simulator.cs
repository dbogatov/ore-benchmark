using System;
using System.Collections.Generic;
using ORESchemes.Shared;
using System.Diagnostics;
using System.Linq;
using ORESchemes.Shared.Primitives;

namespace Simulation.PureSchemes
{
	/// <typeparam name="C">Ciphertext type</typeparam>
	public class Simulator<C>
	{
		private Dictionary<Primitive, long> _primitiveUsage = new Dictionary<Primitive, long>();
		private Dictionary<Primitive, long> _purePrimitiveUsage = new Dictionary<Primitive, long>();

		private IOREScheme<C> _scheme;
		private List<int> _dataset;
		private byte[] _key;

		public Simulator(List<int> dataset, IOREScheme<C> scheme)
		{
			_dataset = dataset;
			_scheme = scheme;
			_key = scheme.KeyGen();

			scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(RecordPrimitiveUsage);

			Enum
				.GetValues(typeof(Primitive))
				.OfType<Primitive>()
				.ToList()
				.ForEach(val =>
				{
					_primitiveUsage.Add(val, 0);
					_purePrimitiveUsage.Add(val, 0);
				});
		}

		/// <summary>
		/// Handler for the event that the primitive has been used
		/// </summary>
		/// <param name="operation">Primitive used</param>
		void RecordPrimitiveUsage(Primitive primitive, bool impure)
		{
			_primitiveUsage[primitive]++;
			if (!impure)
			{
				_purePrimitiveUsage[primitive]++;
			}
		}

		/// <summary>
		/// Generates a report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile</param>
		private Report Profile(Action routine)
		{
			var currentProcess = Process.GetCurrentProcess();
			_primitiveUsage.Keys.ToList().ForEach(key => _primitiveUsage[key] = 0);
			_purePrimitiveUsage.Keys.ToList().ForEach(key => _primitiveUsage[key] = 0);

			var timer = System.Diagnostics.Stopwatch.StartNew();
			var processStartTime = currentProcess.UserProcessorTime;

			routine();

			var processEndTime = currentProcess.UserProcessorTime;
			timer.Stop();

			// for some reason this value is off by exactly hundred
			var procTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round((processEndTime.TotalMilliseconds - processStartTime.TotalMilliseconds) / 100));

			return new Report
			{
				CPUTime = procTime,
				ObservedTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds),
				TotalPrimitiveOperations = _primitiveUsage,
				PurePrimitiveOperations = _purePrimitiveUsage,
				OperationsNumber = _dataset.Count
			};
		}

		/// <summary>
		/// Runs simulation for the dataset and schemes provided through the constructor.
		/// </summary>
		/// <returns>A final report</returns>
		public Report Simulate() =>
			Profile(
				() =>
				{
					for (int i = 1; i < _dataset.Count; i++)
					{
						int a = _dataset[i - 1];
						int b = _dataset[i];

						C ca = _scheme.Encrypt(a, _key);
						C cb = _scheme.Encrypt(b, _key);

						_scheme.Compare(ca, ca);
					}
				}
			);
	}
}
