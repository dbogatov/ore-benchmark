using System;
using ORESchemes.CryptDBOPE;
using ORESchemes.Shared;
using Xunit;

namespace Test.ORESchemes
{
	public class CryptDBOPETests : GenericORETests<long>
	{
		protected override void SetScheme()
		{
			_scheme = new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(Int32.MinValue) * 100,
				Convert.ToInt64(Int32.MaxValue) * 100,
				BitConverter.GetBytes(123456)
			);
		}

		[Fact]
		public void MalformedCiphertext()
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

		[Fact]
		public void SpecialInputs()
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
}
