using System;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace ORESchemes.CryptDBOPE
{
	public struct Range
	{
		public long From { get; set; }
		public long To { get; set; }

		long Size
		{
			get
			{
				return To - From;
			}
		}
	}

	public class CryptDBScheme : IOREScheme<int, long>
	{
		public event SchemeOperationEventHandler OperationOcurred;

		private Range _domain;
		private Range _target;
		private ISampler _sampler;
		private ILFPRF _tapeGen;

		private readonly Random _generator = new Random();
		private readonly int _alpha = 128;

		public CryptDBScheme(
			Range domain,
			Range target,
			ISampler sampler,
			ILFPRF tapeGen,
			int? seed = null,
			int? alpha = null
		)
		{
			if (seed.HasValue)
			{
				_generator = new Random(seed.Value);
			}

			if (alpha.HasValue)
			{
				_alpha = alpha.Value;
			}

			_domain = domain;
			_target = target;
			_sampler = sampler;
			_tapeGen = tapeGen;
		}

		public int Decrypt(long ciphertext, byte[] key)
		{
			throw new NotImplementedException();
		}

		public void Destruct()
		{
			// OnOperation(SchemeOperation.Destruct);

			return;
		}

		public long Encrypt(int plaintext, byte[] key)
		{
			throw new NotImplementedException();
		}

		public void Init()
		{
			throw new NotImplementedException();
		}

		public bool IsEqual(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsGreater(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsGreaterOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsLess(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsLessOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public byte[] KeyGen()
		{
			throw new NotImplementedException();
		}

		public long MaxCiphertextValue()
		{
			throw new NotImplementedException();
		}

		public long MinCiphertextValue()
		{
			throw new NotImplementedException();
		}
	}
}
