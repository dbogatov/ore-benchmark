using System;
using Xunit;
using ORESchemes.Shared;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class NoEncryptionTests : GenericORETests<long, object>
	{
		protected override void SetScheme()
		{
			_scheme = new NoEncryptionScheme();
		}

		[Fact]
		public override void PrimitivesEventsTest()
		{
			_scheme.Init();
			var key = _scheme.KeyGen();

			Dictionary<Primitive, long> primitiveUsage = new Dictionary<Primitive, long>();
			Dictionary<Primitive, long> purePrimitiveUsage = new Dictionary<Primitive, long>();

			Enum
				.GetValues(typeof(Primitive))
				.OfType<Primitive>()
				.ToList()
				.ForEach(val =>
				{
					primitiveUsage.Add(val, 0);
					purePrimitiveUsage.Add(val, 0);
				});

			_scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(primitive, impure) =>
				{
					primitiveUsage[primitive]++;
					if (!impure)
					{
						purePrimitiveUsage[primitive]++;
					}
				}
			);

			_scheme.IsLess(
				_scheme.Encrypt(10, key),
				_scheme.Encrypt(20, key)
			);

			Assert.Equal(0, primitiveUsage.Values.Sum());
			Assert.Equal(0, purePrimitiveUsage.Values.Sum());
		}
	}
}
