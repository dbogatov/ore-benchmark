using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.FHOPE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class FHOPETests : GenericORETests<long>
	{
		protected override void SetScheme()
		{
			_scheme = new FHOPEScheme(long.MinValue, long.MaxValue, _entropy);

			_expectedEvents = new Dictionary<SchemeOperation, Tuple<int, int>>
			{
				{ SchemeOperation.Init, new Tuple<int, int>(1, 1)} ,
				{ SchemeOperation.KeyGen, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Destruct, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Encrypt, new Tuple<int, int>(10, 15) },
				{ SchemeOperation.Decrypt, new Tuple<int, int>(10, 100) },
				{ SchemeOperation.Comparison, new Tuple<int, int>(9 * 5, 9 * 5 * 4) },
			};
		}

		[Fact]
		public override void KeyGenTest()
		{
			_scheme.Init();
			var key = _scheme.KeyGen();

			Assert.Null(key);
		}

		[Fact]
		public void ExceptionsTest()
		{
			FHOPEScheme scheme = Assert.IsType<FHOPEScheme>(_scheme);

			scheme.Init();

			Assert.Throws<InvalidOperationException>(
				() => scheme.Compare(100, 100)
			);

			Assert.Throws<InvalidOperationException>(
				() => scheme.Decrypt(100)
			);

			var min = scheme.Encrypt(int.MinValue);
			var max = scheme.Encrypt(int.MaxValue);

			Assert.Throws<InvalidOperationException>(
				() => scheme.Decrypt(100)
			);

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					scheme.IsLess(100, min);
					scheme.IsLess(max, 100);
				}
			);
		}
	}
}
