using System;
using System.Collections.Generic;
using ORESchemes.Shared;
using System.Diagnostics;
using System.Linq;
using ORESchemes.Shared.Primitives;

namespace Simulation.PureSchemes
{
	/// <typeparam name="C">Ciphertext type</typeparam>
	/// <typeparam name="K">Key type</typeparam>
	public class Simulator<C, K>
	{
		private Dictionary<Primitive, long> _primitiveUsage = new Dictionary<Primitive, long>();
		private Dictionary<Primitive, long> _purePrimitiveUsage = new Dictionary<Primitive, long>();

		private IOREScheme<C, K> _scheme;
		private List<int> _dataset;
		private K _key;

		public Simulator(List<int> dataset, IOREScheme<C, K> scheme)
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
		private Report.Subreport Profile(Action routine)
		{
			var currentProcess = Process.GetCurrentProcess();
			_primitiveUsage.Keys.ToList().ForEach(key => _primitiveUsage[key] = 0);
			_purePrimitiveUsage.Keys.ToList().ForEach(key => _purePrimitiveUsage[key] = 0);

			var timer = System.Diagnostics.Stopwatch.StartNew();
			var processStartTime = currentProcess.UserProcessorTime;

			routine();

			var processEndTime = currentProcess.UserProcessorTime;
			timer.Stop();

			// for some reason this value is off by exactly hundred
			var procTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round((processEndTime.TotalMilliseconds - processStartTime.TotalMilliseconds) / 100));

			return new Report.Subreport
			{
				CPUTime = procTime,
				ObservedTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds),
				TotalPrimitiveOperations = CloneDictionary(_primitiveUsage),
				PurePrimitiveOperations = CloneDictionary(_purePrimitiveUsage),
				OperationsNumber = _dataset.Count
			};
		}

		/// <summary>
		/// Runs simulation for the dataset and schemes provided through the constructor.
		/// </summary>
		/// <returns>A final report</returns>
		public Report Simulate()
		{
			List<C> ciphertexts = new List<C>();

			return new Report
			{
				Encryptions = Profile(() =>
					{
						for (int i = 0; i < _dataset.Count; i++)
						{
							ciphertexts.Add(_scheme.Encrypt(_dataset[i], _key));
						}
					}
				),
				Decryptions = Profile(() =>
					{
						for (int i = 0; i < _dataset.Count; i++)
						{
							_scheme.Decrypt(ciphertexts[i], _key);
						}
					}
				),
				Comparisons = Profile(() =>
					{
						int length = _dataset.Count;
						for (int i = 0; i < length; i++)
						{
							_scheme.IsLess(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
							_scheme.IsLessOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
							_scheme.IsGreater(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
							_scheme.IsGreaterOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
							_scheme.IsEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
						}
					}
				)
			};
		}

		/// <summary>
		/// Produce a deep (not shallow) copy of its argument
		/// </summary>
		/// <param name="original">Dictionary to copy</param>
		private Dictionary<Primitive, long> CloneDictionary(Dictionary<Primitive, long> original)
		{
			Dictionary<Primitive, long> copy = new Dictionary<Primitive, long>();

			original.Keys.ToList().ForEach(key => copy.Add(key, original[key]));

			return copy;
		}
	}
}
