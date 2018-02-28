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
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var generator = new Random();
			var key = _scheme.KeyGen();

			_scheme.Init();

			for (int i = 0; i < _runs; i++)
			{
				var plaintext = new string(
					System.Linq.Enumerable
						.Repeat(chars, 16)
						.Select(s => s[generator.Next(s.Length)])
						.ToArray()
					);

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
		[InlineData("Hello, world!", "Hello, world!", true)]
		[InlineData("123456789", "123456789", true)]
		[InlineData("Hello", "World", false)]
		[InlineData("Hello", "Again!!", false)]
		public void EqualityCorrectnessTest(string plaintextOne, string plaintextTwo, bool equal)
		{
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
		[InlineData("123456789", "123456789", false)]
		[InlineData("0000", "1111", false)]
		[InlineData("1111", "0000", true)]
		public void OrderCorrectnessTest(string plaintextOne, string plaintextTwo, bool equal)
		{
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
