using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes;
using System.Collections.Generic;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class FactoryTests
	{
		[Theory]
		[InlineData(global::ORESchemes.Shared.ORESchemes.CryptDB)]
		[InlineData(global::ORESchemes.Shared.ORESchemes.LewiORE)]
		[InlineData(global::ORESchemes.Shared.ORESchemes.NoEncryption)]
		[InlineData(global::ORESchemes.Shared.ORESchemes.PracticalORE)]
		public void SchemeFactoryTests(global::ORESchemes.Shared.ORESchemes scheme)
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
					case global::ORESchemes.Shared.ORESchemes.CryptDB:
						var cryptdb = new CryptDBOPEFactory(seed).GetScheme();
						Assert.NotNull(cryptdb);
						Assert.IsType<global::ORESchemes.CryptDBOPE.CryptDBScheme>(cryptdb);
						break;
					case global::ORESchemes.Shared.ORESchemes.LewiORE:
						var lewi = new LewiOREFactory(seed).GetScheme();
						Assert.NotNull(lewi);
						Assert.IsType<global::ORESchemes.LewiORE.LewiOREScheme>(lewi);
						break;
					case global::ORESchemes.Shared.ORESchemes.PracticalORE:
						var practical = new PracticalOREFactory(seed).GetScheme();
						Assert.NotNull(practical);
						Assert.IsType<global::ORESchemes.PracticalORE.PracticalOREScheme>(practical);
						break;
					case global::ORESchemes.Shared.ORESchemes.NoEncryption:
						var noEncryption = new NoEncryptionFactory(seed).GetScheme();
						Assert.NotNull(noEncryption);
						Assert.IsType<global::ORESchemes.Shared.NoEncryptionScheme>(noEncryption);
						break;
				}

				useSeed = !useSeed;
			}
		}
	}
}
