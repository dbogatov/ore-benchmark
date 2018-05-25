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
		public void NoExceptions()
		{
			_scheme.Init();

			byte[] key = _scheme.KeyGen();

			for (int i = -9; i <= 10; i++)
			{
				var cipher = _scheme.Encrypt(i, key);
				// Console.Write($"{i} -> {cipher}");

				var decrypted = _scheme.Decrypt(cipher, key);
				// Console.WriteLine($" | {cipher} -> {decrypted}");
			}
		}
	}
}
