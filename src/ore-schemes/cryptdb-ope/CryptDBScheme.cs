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

	public class CryptDBScheme : AbsOREScheme<long>
	{
		private Range _domain;
		private Range _target;
		private ISampler _sampler;
		private ILFPRF _tapeGen;

		public CryptDBScheme(
			Range domain,
			Range target,
			ISampler sampler,
			ILFPRF tapeGen,
			int? seed = null,
			int? alpha = null
		) : base(alpha, seed)
		{
			_domain = domain;
			_target = target;
			_sampler = sampler;
			_tapeGen = tapeGen;
		}

		public override int Decrypt(long ciphertext, byte[] key)
		{
			throw new NotImplementedException();
		}

		public override long Encrypt(int plaintext, byte[] key)
		{
			throw new NotImplementedException();
		}

		protected override bool Compare(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}
	}
}
