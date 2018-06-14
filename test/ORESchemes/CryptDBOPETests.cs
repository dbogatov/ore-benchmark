using System;
using ORESchemes.CryptDBOPE;
using ORESchemes.Shared;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class CryptDBOPERange48Tests : AbsCryptDBOPETests
	{
		protected override void SetParameters() => rangeBits = 48;

		[Fact]
		public void InputOutOfRangeTest()
		{
			var scheme = new CryptDBScheme(
				Int16.MinValue,
				Int16.MaxValue,
				Int16.MinValue,
				Int16.MaxValue,
				_entropy
			);

			scheme.Init();

			byte[] key = scheme.KeyGen();

			Assert.Throws<ArgumentException>(
				() => scheme.Encrypt((int)(Int16.MinValue - 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Encrypt((int)(Int16.MaxValue + 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Decrypt((int)(Int16.MinValue - 10), key)
			);

			Assert.Throws<ArgumentException>(
				() => scheme.Decrypt((int)(Int16.MaxValue + 10), key)
			);
		}

		[Fact]
		/// <summary>
		/// If domain is equal to range, scheme must be one to one
		/// </summary>
		public void OneToOneTest()
		{
			var generator = new Random(SEED);

			var scheme = new CryptDBScheme(
				Int16.MinValue,
				Int16.MaxValue,
				Int16.MinValue,
				Int16.MaxValue,
				_entropy
			);

			scheme.Init();

			byte[] key = scheme.KeyGen();

			for (int i = 0; i < _runs * 100; i++)
			{
				var plaintext = generator.Next(Int16.MinValue, Int16.MaxValue);

				var ciphertext = scheme.Encrypt(plaintext, key);
				Assert.Equal(plaintext, ciphertext);
			}
		}

		[Fact]
		/// <summary>
		/// Inputs known to trigger failures
		/// Every bug must turn to test
		/// </summary>
		public void SpecialInputsTest()
		{
			var entropy = BitConverter.GetBytes(782797714);

			var scheme = new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(Int32.MinValue) * 100000,
				Convert.ToInt64(Int32.MaxValue) * 100000,
				entropy
			);

			scheme.Init();

			byte[] key = scheme.KeyGen();

			var from = scheme.Encrypt(5960, key);
			var to = scheme.Encrypt(6260, key);

			Assert.True(from < to);
		}
	}

	[Trait("Category", "Integration")]
	public class CryptDBOPERange44Tests : AbsCryptDBOPETests
	{
		protected override void SetParameters() => rangeBits = 44;
	}

	[Trait("Category", "Integration")]
	public class CryptDBOPERange40Tests : AbsCryptDBOPETests
	{
		protected override void SetParameters() => rangeBits = 40;
	}

	[Trait("Category", "Integration")]
	public class CryptDBOPERange36Tests : AbsCryptDBOPETests
	{
		protected override void SetParameters() => rangeBits = 36;
	}

	[Trait("Category", "Integration")]
	public class CryptDBOPERange32Tests : AbsCryptDBOPETests
	{
		protected override void SetParameters() => rangeBits = 32;
	}

	public abstract class AbsCryptDBOPETests : GenericORETests<long, byte[]>
	{
		protected int rangeBits = 48;

		protected override void SetScheme()
		{
			_scheme = new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(-Math.Pow(2, rangeBits)),
				Convert.ToInt64(Math.Pow(2, rangeBits)),
				_entropy
			);
		}

		[Fact]
		public void MalformedCiphertextTest()
		{
			_scheme.Init();

			byte[] key = _scheme.KeyGen();

			long from = 0;
			long to = 0;

			for (int i = 0; i < 10; i++)
			{
				from = _scheme.Encrypt(50 + i, key);
				to = _scheme.Encrypt(51 + i, key);

				if (to - from > 1)
				{
					break;
				}
			}

			Assert.True(to - from > 1);

			Assert.Throws<ArgumentException>(
				() => _scheme.Decrypt(from + 1, key)
			);
		}
	}
}
