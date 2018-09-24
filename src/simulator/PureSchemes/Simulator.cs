using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
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

			perQuery = new Tracker();
			perStage = new Tracker();

			scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(RecordPrimitiveUsage);
		}

		/// <summary>
		/// Generates a report filled with data gathered during the execution of 
		/// given function. Records times and number of events.
		/// </summary>
		/// <param name="routine">Function to profile (must take ciphers, dataset, scheme and key)</param>
		/// <param name="ciphers">List of ciphertext to populate or to take dta from</param>
		/// <param name="stage">Stage of simulation (affects how averages are computed)</param>
		/// <returns>A subreport generated for this particular profiling</returns>
		private Report.SubReport Profile(Action<List<C>, List<int>, IOREScheme<C, K>, K> routine, List<C> ciphers, Stages stage)
		{
			TimerHandler(stop: false);

			routine(ciphers, _dataset, _scheme, _key);

			TimerHandler(stop: true);

			RecordMaxCipherStateSize(isState: true, _key.GetSize());
			RecordMaxCipherStateSize(isState: false, ciphers.Max(c => c.GetSize()));

			var report = (Report.SubReport)StageReport();

			report.SchemeOperations = _dataset.Count * (stage != Stages.Compare ? 1 : 5);

			return report;
		}

		public override AbsReport<Stages> Simulate()
		{
			List<C> ciphertexts = new List<C>();

			var result = new Report();

			result.Stages[Stages.Encrypt] = Profile(EncryptStage, ciphertexts, Stages.Encrypt);
			result.Stages[Stages.Decrypt] = Profile(DecryptStage, ciphertexts, Stages.Decrypt);
			result.Stages[Stages.Compare] = Profile(CompareStage, ciphertexts, Stages.Compare);

			return result;
		}

		/// <summary>
		/// Encryption stage simulation
		/// </summary>
		/// <param name="ciphertexts">Ciphertext list to populate</param>
		/// <param name="dataset">Dataset from which to take data for operations</param>
		/// <param name="scheme">Scheme to perform operations with</param>
		/// <param name="key">Key to use with the scheme</param>
		public static void EncryptStage(List<C> ciphertexts, List<int> dataset, IOREScheme<C, K> scheme, K key)
		{
			for (int i = 0; i < dataset.Count; i++)
			{
				ciphertexts.Add(scheme.Encrypt(dataset[i], key));
			}
		}

		/// <summary>
		/// Decryption stage simulation
		/// </summary>
		/// <param name="ciphertexts">Ciphertext list to take inputs from</param>
		/// <param name="dataset">Unused</param>
		/// <param name="scheme">Scheme to perform operations with</param>
		/// <param name="key">Key to use with the scheme</param>
		public static void DecryptStage(List<C> ciphertexts, List<int> dataset, IOREScheme<C, K> scheme, K key)
		{
			for (int i = 0; i < dataset.Count; i++)
			{
				scheme.Decrypt(ciphertexts[i], key);
			}
		}

		/// <summary>
		/// Comparison stage simulation
		/// </summary>
		/// <param name="ciphertexts">Ciphertext list to take inputs from</param>
		/// <param name="dataset">Unused</param>
		/// <param name="scheme">Scheme to perform operations with</param>
		/// <param name="key">Key to use with the scheme</param>
		public static void CompareStage(List<C> ciphertexts, List<int> dataset, IOREScheme<C, K> scheme, K key)
		{
			int length = dataset.Count;

			// This is a necessary hack because FH-OPE requires min and max 
			// ciphers for comparison which can be computed only when all
			// plaintexts got encrypted
			if (scheme is ORESchemes.FHOPE.FHOPEScheme)
			{
				var fhope = (ORESchemes.FHOPE.FHOPEScheme)scheme;
				ciphertexts.ForEach(
					c =>
					{
						int plaintext = scheme.Decrypt(c, key);
						((ORESchemes.FHOPE.Ciphertext)(object)c).max = fhope.MaxCiphertext(plaintext, (ORESchemes.FHOPE.State)(object)key);
						((ORESchemes.FHOPE.Ciphertext)(object)c).min = fhope.MinCiphertext(plaintext, (ORESchemes.FHOPE.State)(object)key);
					}
				);
			}

			for (int i = 0; i < length; i++)
			{
				scheme.IsLess(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
				scheme.IsLessOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
				scheme.IsGreater(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
				scheme.IsGreaterOrEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
				scheme.IsEqual(ciphertexts[i % length], ciphertexts[(i + 1) % length]);
			}
		}
	}

	public class Tracker : AbsTracker
	{
		public override AbsSubReport ReadMetrics()
			=> new Report.SubReport
			{
				ObservedTime = _totalTime,
				TotalPrimitiveOperations = CloneDictionary(_primitiveUsage),
				PurePrimitiveOperations = CloneDictionary(_purePrimitiveUsage),
				MaxCipherSize = _maxCipherSize,
				MaxStateSize = _maxStateSize
			};
	}
}
