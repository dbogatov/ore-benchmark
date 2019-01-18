using Crypto.CLOZ;
using Xunit;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class CLOZ : GenericORE<Ciphertext, Key>
	{
		protected override void SetScheme()
		{
			_scheme = new Scheme(_entropy);
		}

		public override int CipherSize() => 4088;
		public override int KeySize() => 384;
	}
}
