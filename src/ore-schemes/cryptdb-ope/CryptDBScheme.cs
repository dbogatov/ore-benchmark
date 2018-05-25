using System;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace ORESchemes.CryptDBOPE
{
	public struct Range
	{
		public long From { get; set; }
		public long To { get; set; }

		public long Size
		{
			get
			{
				return To - From;
			}
		}

		public long Min
		{
			get
			{
				return Math.Min(To, From);
			}
		}

		public override int GetHashCode()
		{
			return
				5 * From.GetHashCode() +
				7 * To.GetHashCode();
		}
	}

	public class CryptDBScheme : AbsOREScheme<long>
	{
		private Range _domain;
		private Range _target;

		public CryptDBScheme(
			Range domain,
			Range target,
			byte[] seed = null
		) : base(seed)
		{
			_domain = domain;
			_target = target;
		}

		public override int Decrypt(long ciphertext, byte[] key)
		{
			throw new NotImplementedException();
		}

		public override long Encrypt(int plaintext, byte[] key)
		{
			var M = _domain.Size;
			var N = _target.Size;

			var d = _domain.Min - 1;
			var r = _target.Min - 1;

			byte[] input;
			byte[] cc;

			var y = r + (int)Math.Ceiling((decimal)N / (decimal)2.0);

			if (M == 1)
			{
				input =
					BitConverter.GetBytes(_domain.GetHashCode())
						.Concat(BitConverter.GetBytes(_target.GetHashCode()))
						.Concat(BitConverter.GetBytes((int)1))
						.Concat(BitConverter.GetBytes(plaintext))
						.ToArray();

				cc = LFPRFFactory.GetLFPRF().Generate(key, sizeof(long), input);

				// TODO
				// Use whole cc to sample
				// var uniform = SamplerFactory.GetSampler(cc.GetProperHashCode());

				var c = SamplerFactory.GetSampler(cc).Uniform(_target.From, _target.To); //_target.SampleUniform(uniform);

				return c;
			}

			input =
					BitConverter.GetBytes(_domain.GetHashCode())
						.Concat(BitConverter.GetBytes(_target.GetHashCode()))
						.Concat(BitConverter.GetBytes((int)0))
						.Concat(BitConverter.GetBytes(y))
						.ToArray();

			cc = LFPRFFactory.GetLFPRF().Generate(key, sizeof(long), input);

			// TODO
			// Check params
			var x = d + (long)SamplerFactory.GetSampler().HyperGeometric((ulong)N, (ulong)M, (ulong)(y - r));

			// if (m <= x)
			// {

			// }

			return 0;
		}

		protected override bool Compare(long ciphertextOne, long ciphertextTwo)
		{
			throw new NotImplementedException();
		}
	}
}
