using System;
using ORESchemes.CryptDBOPE;
using ORESchemes.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class CryptDBOPE : AbsBPlusTreeTests<OPECipher, BytesKey>
	{
		public CryptDBOPE() : base(
			new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(Int32.MinValue) * 100000,
				Convert.ToInt64(Int32.MaxValue) * 100000,
				new byte[] { 13, 05, 19, 96 }
			), 100
		)
		{ }
	}
}
