using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.Sampler;
using ORESchemes.Shared.Primitives.TapeGen;

namespace ORESchemes.CryptDBOPE
{
	/// <summary>
	/// Implemented as in https://eprint.iacr.org/2012/624.pdf
	/// </summary>
	public class CryptDBScheme : AbsOPEScheme
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

		/// <summary>
		/// Constructor requires domain, range and optionally 256 bytes of entropy.
		/// Although domain and range are signed integers, internally, scheme will use unsigned versions.
		/// </summary>
		/// <returns></returns>
		public CryptDBScheme(
			int domainFrom,
			int domainTo,
			long targetFrom,
			long targetTo,
			byte[] entropy = null
		) : base(entropy)
		{
			_domain.From = ToUInt(domainFrom);
			_domain.To = ToUInt(domainTo);

			_target.From = ToULong(targetFrom);
			_target.To = ToULong(targetTo);
		}

		public override int Decrypt(long ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			ulong c = ToULong(ciphertext);

			if (c > _target.To || c < _target.From)
			{
				throw new ArgumentException($"Scheme was initialized with range [{ToLong(_target.From)}, {ToLong(_target.To)}]");
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
				TapeGen tape;

				if (M == 1)
				{
					ulong m = domain.From;

					input = Concatenate(domain, target, true, (ulong)m);

					tape = new TapeGen(key, input);

					ulong uniform = SamplerFactory.GetSampler(tape).Uniform(target.From, target.To);

					if (uniform == c)
					{
						return ToInt((uint)m);
					}
					else
					{
						throw new ArgumentException($"Ciphertext {ciphertext} has never been a result of an encryption.");
					}
				}

				input = Concatenate(domain, target, false, y);

				tape = new TapeGen(key, input);

				ulong hg = SamplerFactory.GetSampler(tape).HyperGeometric((ulong)N, (ulong)(y - r), (ulong)M);
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

				if (domain.Size < 1)
				{
					throw new InvalidOperationException("Should never happen.");
				}
			}

			throw new InvalidOperationException("Should never reach this.");
		}

		public override long Encrypt(int plaintext, byte[] key)
		{
			OnOperation(SchemeOperation.Encrypt);

			uint m = ToUInt(plaintext);

			if (m > _domain.To || m < _domain.From)
			{
				throw new ArgumentException($"Scheme was initialized with domain [{ToInt((uint)_domain.From)}, {ToInt((uint)_domain.To)}]");
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
				TapeGen tape;

				if (M == 1)
				{
					input = Concatenate(domain, target, true, (ulong)m);

					tape = new TapeGen(key, input);

					ulong uniform = SamplerFactory.GetSampler(tape).Uniform(target.From, target.To);

					return ToLong(uniform);
				}

				input = Concatenate(domain, target, false, y);

				tape = new TapeGen(key, input);

				ulong hg = SamplerFactory.GetSampler(tape).HyperGeometric((ulong)N, (ulong)(y - r), (ulong)M);				
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

				if (domain.Size < 1)
				{
					throw new InvalidOperationException("Should never happen.");
				}
			}

			throw new InvalidOperationException("Should never reach this.");
		}

		public override int MaxPlaintextValue() => ToInt((uint)_domain.To);
		public override int MinPlaintextValue() => ToInt((uint)_domain.From);

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

		/// <summary>
		/// Transforms signed int32 to unsigned int32 by shifting the value by int32 min value
		/// </summary>
		private uint ToUInt(int value) => unchecked((uint)(value + Int32.MinValue));

		/// <summary>
		/// Transforms signed int64 to unsigned int64 by shifting the value by int64 min value
		/// </summary>
		private ulong ToULong(long value) => unchecked((ulong)(value + Int64.MinValue));

		/// <summary>
		/// Transforms unsigned int32 to signed int32 by shifting the value by int32 min value
		/// </summary>
		private int ToInt(uint value) => (int)(value - Int32.MinValue);

		/// <summary>
		/// Transforms unsigned int64 to signed int64 by shifting the value by int64 min value
		/// </summary>
		private long ToLong(ulong value) => (long)(value - unchecked((ulong)Int64.MinValue));
	}
}
