using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ORESchemes.Shared;
using Simulation;

namespace Benchmark.Schemes
{
	/// <typeparam name="C">Ciphertext type</typeparam>
	/// <typeparam name="K">Key type</typeparam>
	public class Benchmark<C, K>
		where C : IGetSize
		where K : IGetSize
	{
		private enum Stages
		{
			Encrypt, Decrypt, Compare
		}

		private List<int> _dataset;
		private IOREScheme<C, K> _scheme;

		private Dictionary<Stages, List<C>> _ciphertexts = new Dictionary<Stages, List<C>>();
		private Dictionary<Stages, K> _keys = new Dictionary<Stages, K>();

		[ParamsSource(nameof(Parameters))]
		public Tuple<ORESchemes.Shared.ORESchemes, int> Scheme;

		[GlobalSetup]
		public void GlobalSetup()
		{
			const int RUNS = 10; // For now, manually change it. See https://github.com/dotnet/BenchmarkDotNet/issues/808
			var seed = 123456;
			var random = new Random(seed);

			_dataset = Enumerable.Repeat(0, RUNS).Select(v => random.Next(int.MinValue, int.MaxValue)).ToList();

			switch (Scheme.Item1)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					_scheme = (IOREScheme<C, K>)new NoEncryptionFactory(seed).GetScheme(Scheme.Item2);
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					_scheme = (IOREScheme<C, K>)new CryptDBOPEFactory(seed).GetScheme(Scheme.Item2);
					break;
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					_scheme = (IOREScheme<C, K>)new PracticalOREFactory(seed).GetScheme(Scheme.Item2);
					break;
				case ORESchemes.Shared.ORESchemes.LewiORE:
					_scheme = (IOREScheme<C, K>)new LewiOREFactory(seed).GetScheme(Scheme.Item2);
					break;
				case ORESchemes.Shared.ORESchemes.FHOPE:
					_scheme = (IOREScheme<C, K>)new FHOPEFactory(seed).GetScheme(Scheme.Item2);
					break;
			}

			foreach (var stage in Enum.GetValues(typeof(Stages)).Cast<Stages>())
			{
				_keys.Add(stage, _scheme.KeyGen());
				_ciphertexts.Add(stage, new List<C>());
			}
		}

		[IterationSetup(Target = nameof(Encrypt))]
		public void IterationSetupEncrypt() => IterationSetup(Stages.Encrypt);

		[IterationSetup(Target = nameof(Decrypt))]
		public void IterationSetupDecrypt() => IterationSetup(Stages.Decrypt);

		[IterationSetup(Target = nameof(Compare))]
		public void IterationSetupCompare() => IterationSetup(Stages.Compare);

		[Benchmark]
		public void Encrypt() => Simulation.PureSchemes.Simulator<C, K>.EncryptStage(_ciphertexts[Stages.Encrypt], _dataset, _scheme, _keys[Stages.Encrypt]);

		[Benchmark]
		public void Decrypt() => Simulation.PureSchemes.Simulator<C, K>.DecryptStage(_ciphertexts[Stages.Decrypt], _dataset, _scheme, _keys[Stages.Decrypt]);

		[Benchmark]
		public void Compare() => Simulation.PureSchemes.Simulator<C, K>.CompareStage(_ciphertexts[Stages.Compare], _dataset, _scheme, _keys[Stages.Compare]);

		public IEnumerable<object> Parameters()
		{
			if (typeof(C) == typeof(OPECipher))
			{
				yield return new Tuple<ORESchemes.Shared.ORESchemes, int>(ORESchemes.Shared.ORESchemes.NoEncryption, 0);
				foreach (var value in new int[] { 32, 36, 40, 44, 48 })
				{
					yield return new Tuple<ORESchemes.Shared.ORESchemes, int>(ORESchemes.Shared.ORESchemes.CryptDB, value);
				}
			}
			else if (typeof(C) == typeof(ORESchemes.PracticalORE.Ciphertext))
			{
				yield return new Tuple<ORESchemes.Shared.ORESchemes, int>(ORESchemes.Shared.ORESchemes.PracticalORE, 0);
			}
			else if (typeof(C) == typeof(ORESchemes.LewiORE.Ciphertext))
			{
				foreach (var value in new int[] { 16, 8, 4 })
				{
					yield return new Tuple<ORESchemes.Shared.ORESchemes, int>(ORESchemes.Shared.ORESchemes.LewiORE, value);
				}
			}
			else if (typeof(C) == typeof(ORESchemes.FHOPE.Ciphertext))
			{
				foreach (var value in new int[] { 0, 50 })
				{
					yield return new Tuple<ORESchemes.Shared.ORESchemes, int>(ORESchemes.Shared.ORESchemes.FHOPE, value);
				}
			}
		}

		private void IterationSetup(Stages stage)
		{
			_ciphertexts[stage].Clear();
			_keys[stage] = _scheme.KeyGen();

			if (stage != Stages.Encrypt)
			{
				Simulation.PureSchemes.Simulator<C, K>.EncryptStage(_ciphertexts[stage], _dataset, _scheme, _keys[stage]);
			}
		}
	}
}
