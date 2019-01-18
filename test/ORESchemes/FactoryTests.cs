using System;
using Xunit;
using Simulation;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class Factory
	{
		[Theory]
		[InlineData(global::Crypto.Shared.Protocols.BCLO)]
		[InlineData(global::Crypto.Shared.Protocols.LewiWu)]
		[InlineData(global::Crypto.Shared.Protocols.NoEncryption)]
		[InlineData(global::Crypto.Shared.Protocols.CLWW)]
		[InlineData(global::Crypto.Shared.Protocols.FHOPE)]
		[InlineData(global::Crypto.Shared.Protocols.CLOZ)]
		public void SchemeFactory(global::Crypto.Shared.Protocols scheme)
		{
			bool useSeed = true;

			for (int i = 0; i < 2; i++)
			{
				int? seed = null;
				if (useSeed)
				{
					seed = new Random(123456).Next();
				}

				switch (scheme)
				{
					case global::Crypto.Shared.Protocols.BCLO:
						var bclo = new BCLOFactory(seed).GetScheme();
						Assert.NotNull(bclo);
						Assert.IsType<global::Crypto.BCLO.Scheme>(bclo);
						break;
					case global::Crypto.Shared.Protocols.LewiWu:
						var lewiWu = new LewiWuFactory(seed).GetScheme();
						Assert.NotNull(lewiWu);
						Assert.IsType<global::Crypto.LewiWu.Scheme>(lewiWu);
						break;
					case global::Crypto.Shared.Protocols.CLWW:
						var clww = new CLWWFactory(seed).GetScheme();
						Assert.NotNull(clww);
						Assert.IsType<global::Crypto.CLWW.Scheme>(clww);
						break;
					case global::Crypto.Shared.Protocols.NoEncryption:
						var noEncryption = new NoEncryptionFactory(seed).GetScheme();
						Assert.NotNull(noEncryption);
						Assert.IsType<global::Crypto.Shared.NoEncryptionScheme>(noEncryption);
						break;
					case global::Crypto.Shared.Protocols.FHOPE:
						var fhope = new FHOPEFactory(seed).GetScheme();
						Assert.NotNull(fhope);
						Assert.IsType<global::Crypto.FHOPE.Scheme>(fhope);
						break;
					case global::Crypto.Shared.Protocols.CLOZ:
						var cloz = new CLOZFactory(seed).GetScheme();
						Assert.NotNull(cloz);
						Assert.IsType<global::Crypto.CLOZ.Scheme>(cloz);
						break;
				}

				useSeed = !useSeed;
			}
		}
	}
}
