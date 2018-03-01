using System;
using Xunit;
using OPESchemes;
using System.Linq;
using System.Threading;

namespace test
{
	public class NoEncryptionTests
	{
		private readonly IOPEScheme _scheme = new NoEncryptionScheme();
		private readonly int _runs = 10;

		[Fact]
		public void InitTest()
		{
			_scheme.Init();
		}

		[Fact]
		public void DestructTest()
		{
			_scheme.Destruct();
		}

		[Fact]
		public void KeyGenTest()
		{
			var keyOne = _scheme.KeyGen();

			Thread.Sleep(50);

			var keyTwo = _scheme.KeyGen();

			Assert.NotEqual(keyOne, keyTwo);
		}

		[Fact]
		/// <summary>
		/// Decryption of encryption should be original plaintext for all
		/// valid keys and plaintexts
		/// </summary>
		public void CorrectnessTest()
		{
			_scheme.Init();

			var generator = new Random();
			var key = _scheme.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				var plaintext = generator.Next(Int32.MaxValue);

				Assert.Equal(
					_scheme.Decrypt(
						_scheme.Encrypt(plaintext, key),
						key
					),
					plaintext
				);

				Thread.Sleep(50);
			}
		}

		[Theory]
		[InlineData(0, 0, true)]
		[InlineData(-1, -1, true)]
		[InlineData(1, 1, true)]
		[InlineData(-1, 1, false)]
		[InlineData(1, 2, false)]
		public void EqualityCorrectnessTest(int plaintextOne, int plaintextTwo, bool equal)
		{
			_scheme.Init();

			var key = _scheme.KeyGen();
			var ciphertextOne = _scheme.Encrypt(plaintextOne, key);
			var ciphertextTwo = _scheme.Encrypt(plaintextTwo, key);

			var result = _scheme.IsEqual(ciphertextOne, ciphertextTwo);

			if (equal)
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[Theory]
		[InlineData(1, 1, false)]
		[InlineData(-1, 1, false)]
		[InlineData(1, -1, true)]
		[InlineData(2, 1, true)]
		public void OrderCorrectnessTest(int plaintextOne, int plaintextTwo, bool equal)
		{
			_scheme.Init();

			var key = _scheme.KeyGen();
			var ciphertextOne = _scheme.Encrypt(plaintextOne, key);
			var ciphertextTwo = _scheme.Encrypt(plaintextTwo, key);

			var result = _scheme.IsGreater(ciphertextOne, ciphertextTwo);

			if (equal)
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}
	}
}
