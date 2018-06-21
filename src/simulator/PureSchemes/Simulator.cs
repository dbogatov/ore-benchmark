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
	public class Simulator<C, K> : AbsSimulator<Stages>
		where C : IGetSize
		where K : IGetSize
	{
		private IOREScheme<C, K> _scheme;
		private List<int> _dataset;
		private K _key;

		public Simulator(List<int> dataset, IOREScheme<C, K> scheme)
		{
			_dataset = dataset;
			_scheme = scheme;
			_key = scheme.KeyGen();

			scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(RecordPrimitiveUsage);
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
				SchemeOperations = _dataset.Count
			};
		}

		public override AbsReport<Stages> Simulate()
		{
			List<C> ciphertexts = new List<C>();

			var result = new Report();

			result.Stages[Stages.Encrypt] = Profile(() =>
				{
					for (int i = 0; i < _dataset.Count; i++)
					{
						ciphertexts.Add(_scheme.Encrypt(_dataset[i], _key));
					}
				}
			);
			result.Stages[Stages.Decrypt] = Profile(() =>
				{
					for (int i = 0; i < _dataset.Count; i++)
					{
						_scheme.Decrypt(ciphertexts[i], _key);
					}
				}
			);
			result.Stages[Stages.Compare] = Profile(() =>
				{
					int length = _dataset.Count;

					// This is a necessary hack because FH-OPE requires min and max 
					// ciphers for comparison which can be computed only when all
					// plaintexts got encrypted
					if (_scheme is ORESchemes.FHOPE.FHOPEScheme)
					{
						var scheme = (ORESchemes.FHOPE.FHOPEScheme)_scheme;
						ciphertexts.ForEach(
							c =>
							{
								int plaintext = _scheme.Decrypt(c, _key);
								((ORESchemes.FHOPE.Ciphertext)(object)c).max = scheme.MaxCiphertext(plaintext, (ORESchemes.FHOPE.State)(object)_key);
								((ORESchemes.FHOPE.Ciphertext)(object)c).min = scheme.MinCiphertext(plaintext, (ORESchemes.FHOPE.State)(object)_key);
							}
						);
					}

					for (int i = 0; i < length; i++)
					{
						_scheme.IsLess(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
						_scheme.IsLessOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
						_scheme.IsGreater(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
						_scheme.IsGreaterOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
						_scheme.IsEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
					}
				}
			);

			return result;
		}
	}
}
