using System;
using ORESchemes.CryptDBOPE;
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
				unchecked((long)Int32.MinValue * 100),
				unchecked((long)Int32.MaxValue * 100),
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
	}
}
