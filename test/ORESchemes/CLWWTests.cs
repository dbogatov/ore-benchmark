using Crypto.CLWW;
using Crypto.Shared;
using Xunit;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class CLWW : GenericORE<Ciphertext, BytesKey>
	{
		protected override void SetScheme()
		{
			_scheme = new Scheme(_entropy);
		}

		public override int CipherSize() => 32 * 2;
	}
}
