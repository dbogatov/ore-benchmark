using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Crypto.Shared;
using Crypto.Shared.Primitives.Sampler;
using Crypto.Shared.Primitives.TapeGen;

namespace Crypto.BCLO
{
	/// <summary>
	/// Implemented as in https://eprint.iacr.org/2012/624.pdf
	/// </summary>
	public class Scheme : AbsOPEScheme<BytesKey>
	{
		private struct Range
		{
			public ulong From { get; set; }
			public ulong To { get; set; }

			public ulong Size
			{
				get
				{
					return To - From + 1;
				}
			}
		}

		private Range _domain;
		private Range _target;

		private ISampler _s;
		private ISampler S
		{
			get => _s;
			set
			{
				SubscribePrimitive(value);
				_s = value;
			}
		}

		private TapeGen _t;
		private TapeGen T
		{
			get => _t;
			set
			{
				SubscribePrimitive(value);
				_t = value;
			}
		}

		/// <summary>
		/// Constructor requires domain, range and optionally 128 bytes of entropy.
		/// Although domain and range are signed integers, internally, scheme will use unsigned versions.
		/// </summary>
		/// <returns></returns>
		public Scheme(
			int domainFrom,
			int domainTo,
			long targetFrom,
			long targetTo,
			byte[] entropy = null
		) : base(entropy)
		{
			_domain.From = domainFrom.ToUInt();
			_domain.To = domainTo.ToUInt();

			_target.From = targetFrom.ToULong();
			_target.To = targetTo.ToULong();
		}

		public override int Decrypt(OPECipher ciphertext, BytesKey key)
		{
			OnOperation(SchemeOperation.Decrypt);

			ulong c = ciphertext.value.ToULong();

			if (c > _target.To || c < _target.From)
			{
				throw new ArgumentException($"Scheme was initialized with range [{(_target.From).ToLong()}, {(_target.To).ToLong()}]");
			}

			Range domain = _domain;
			Range target = _target;

			while (true)
			{
				ulong M = domain.Size;
				ulong N = target.Size;

				ulong d = domain.From - 1;
				ulong r = target.From - 1;

				ulong y = r + (ulong)Math.Ceiling(N / 2.0M);

				byte[] input;

				if (M == 1)
				{
					ulong m = domain.From;

					input = Concatenate(domain, target, true, (ulong)m);

					T = new TapeGen(key.value, input);

					S = new SamplerFactory(T).GetPrimitive();
					ulong uniform = S.Uniform(target.From, target.To);

					if (uniform == c)
					{
						return ((uint)m).ToInt();
					}
					else
					{
						throw new ArgumentException($"Ciphertext {ciphertext} has never been a result of an encryption.");
					}
				}

				input = Concatenate(domain, target, false, y);

				T = new TapeGen(key.value, input);

				S = new SamplerFactory(T).GetPrimitive();
				ulong hg = S.HyperGeometric((ulong)N, (ulong)(y - r), (ulong)M);
				ulong x = d + hg;

				// Special case when integer overflow affects the logic
				if (c <= y && !(c == 0 && y == UInt64.MaxValue))
				{
					domain.To = x;
					target.To = y;
				}
				else
				{
					domain.From = x + 1;
					target.From = y + 1;
				}

				Debug.Assert(domain.Size >= 1);
			}
		}

		public override OPECipher Encrypt(int plaintext, BytesKey key)
		{
			OnOperation(SchemeOperation.Encrypt);

			uint m = plaintext.ToUInt();

			if (m > _domain.To || m < _domain.From)
			{
				throw new ArgumentException($"Scheme was initialized with domain [{((uint)_domain.From).ToInt()}, {((uint)_domain.To).ToInt()}]");
			}

			Range domain = _domain;
			Range target = _target;

			while (true)
			{
				ulong M = domain.Size;
				ulong N = target.Size;

				ulong d = domain.From - 1;
				ulong r = target.From - 1;

				ulong y = r + (ulong)Math.Ceiling(N / 2.0M);

				byte[] input;

				if (M == 1)
				{
					input = Concatenate(domain, target, true, (ulong)m);

					T = new TapeGen(key.value, input);

					S = new SamplerFactory(T).GetPrimitive();
					ulong uniform = S.Uniform(target.From, target.To);

					return new OPECipher(uniform.ToLong());
				}

				input = Concatenate(domain, target, false, y);

				T = new TapeGen(key.value, input);

				S = new SamplerFactory(T).GetPrimitive();
				ulong hg = S.HyperGeometric((ulong)N, (ulong)(y - r), (ulong)M);
				ulong x = d + hg;

				// Special case when integer overflow affects the logic
				if (m <= x && !(m == 0 && x == UInt64.MaxValue))
				{
					domain.To = x;
					target.To = y;
				}
				else
				{
					domain.From = x + 1;
					target.From = y + 1;
				}

				Debug.Assert(domain.Size >= 1);
			}
		}

		/// <summary>
		/// Helper function that performes a convertible operation on its inputs.
		/// namely, concatenates them to a byte array.
		/// </summary>
		/// <param name="domain">Domain object</param>
		/// <param name="target">Range object</param>
		/// <param name="input">True, if plaintext supplied, false otherwise</param>
		/// <param name="value">Plaintext, or other value to add to array</param>
		/// <returns>A concatenation of inputs</returns>
		private byte[] Concatenate(Range domain, Range target, bool input, ulong value)
		{
			var bytes = new List<ulong>
			{
				domain.From,
				domain.To,
				target.From,
				target.To,
				input ? 1UL : 0UL,
				value
			}
			.Select(number => BitConverter.GetBytes(number))
			.ToArray();

			byte[] result = new byte[0];

			for (int i = 0; i < bytes.Count(); i++)
			{
				result = result.Concat(bytes[i]).ToArray();
			}

			return result;
		}

		public override BytesKey KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[ALPHA / 8];
			G.NextBytes(key);

			return new BytesKey(key);
		}
	}
}
