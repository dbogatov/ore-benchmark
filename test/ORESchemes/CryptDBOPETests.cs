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
				-9, 10,
				-99, 100,
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
				Console.WriteLine($"{i} -> {cipher}");
			}
		}
	}
}
