using System;
using Crypto.BCLO;
using Crypto.Shared;
using Xunit;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class BCLORange48 : AbsBCLO
	{
		protected override void SetParameters() => rangeBits = 48;

		[Fact]
		public void InputOutOfRange()
		{
			var scheme = new Scheme(
				Int16.MinValue,
				Int16.MaxValue,
				Int16.MinValue,
				Int16.MaxValue,
				_entropy
			);

			BytesKey key = scheme.KeyGen();

			Assert.Throws<ArgumentException>(
				() => scheme.Encrypt((int)(Int16.MinValue - 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Encrypt((int)(Int16.MaxValue + 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Decrypt(new OPECipher(Int16.MinValue - 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Decrypt(new OPECipher(Int16.MaxValue + 10), key)
			);
		}

		[Fact]
		/// <summary>
		/// If domain is equal to range, scheme must be one to one
		/// </summary>
		public void OneToOne()
		{
			var generator = new Random(SEED);

			var scheme = new Scheme(
				Int16.MinValue,
				Int16.MaxValue,
				Int16.MinValue,
				Int16.MaxValue,
				_entropy
			);

			BytesKey key = scheme.KeyGen();

			for (int i = 0; i < _runs * 100; i++)
			{
				var plaintext = generator.Next(Int16.MinValue, Int16.MaxValue);

				var ciphertext = scheme.Encrypt(plaintext, key);
				Assert.Equal(plaintext, ciphertext.value);
			}
		}

		[Fact]
		/// <summary>
		/// Inputs known to trigger failures
		/// Every bug must turn to test
		/// </summary>
		public void SpecialInputs()
		{
			var entropy = BitConverter.GetBytes(782797714);

			var scheme = new Scheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(Int32.MinValue) * 100000,
				Convert.ToInt64(Int32.MaxValue) * 100000,
				entropy
			);

			BytesKey key = scheme.KeyGen();

			var from = scheme.Encrypt(5960, key);
			var to = scheme.Encrypt(6260, key);

			Assert.True(from < to);
		}
	}

	[Trait("Category", "Integration")]
	public class BCLORange44 : AbsBCLO
	{
		protected override void SetParameters() => rangeBits = 44;
	}

	[Trait("Category", "Integration")]
	public class BCLORange40 : AbsBCLO
	{
		protected override void SetParameters() => rangeBits = 40;
	}

	[Trait("Category", "Integration")]
	public class BCLORange36 : AbsBCLO
	{
		protected override void SetParameters() => rangeBits = 36;
	}

	[Trait("Category", "Integration")]
	public class BCLORange32 : AbsBCLO
	{
		protected override void SetParameters() => rangeBits = 32;
	}

	public abstract class AbsBCLO : GenericORE<OPECipher, BytesKey>
	{
		protected int rangeBits = 48;

		protected override void SetScheme()
		{
			_scheme = new Scheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(-Math.Pow(2, rangeBits)),
				Convert.ToInt64(Math.Pow(2, rangeBits)),
				_entropy
			);
		}

		[Fact]
		public void MalformedCiphertext()
		{
			BytesKey key = _scheme.KeyGen();

			OPECipher from = new OPECipher(0);
			OPECipher to = new OPECipher(0);

			for (int i = 0; i < 10; i++)
			{
				from = _scheme.Encrypt(50 + i, key);
				to = _scheme.Encrypt(51 + i, key);

				if (to.value - from.value > 1)
				{
					break;
				}
			}

			Assert.True(to.value - from.value > 1);

			// It may happen that domain gets zero size or uniform check fails
			Assert.ThrowsAny<Exception>(
				() => _scheme.Decrypt(new OPECipher(from.value + 1), key)
			);
		}
	}
}
