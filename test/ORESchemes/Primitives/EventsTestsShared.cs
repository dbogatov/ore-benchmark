using Xunit;
using ORESchemes.Shared.Primitives;
using System.Collections.Generic;
using System;
using System.Linq;
using ORESchemes.Shared.Primitives.PRF;

namespace Test.ORESchemes.Primitives
{
	public static class EventsTestsShared
	{
		public static void EventsTests<T>(
			T prim,
			Action<T> routine,
			Dictionary<Primitive, int> expectedTotal,
			Dictionary<Primitive, int> expectedPure
		) where T : IPrimitive
		{
			var actualTotal = new Dictionary<Primitive, int>();
			var actualPure = new Dictionary<Primitive, int>();

			foreach (var primitive in Enum.GetValues(typeof(Primitive)).Cast<Primitive>())
			{
				actualTotal.Add(primitive, 0);
				actualPure.Add(primitive, 0);

				if (!expectedTotal.ContainsKey(primitive))
				{
					expectedTotal.Add(primitive, 0);
				}

				if (!expectedPure.ContainsKey(primitive))
				{
					expectedPure.Add(primitive, 0);
				}
			}

			prim.PrimitiveUsed += new PrimitiveUsageEventHandler((p, impure) =>
			{
				actualTotal[p]++;
				if (!impure)
				{
					actualPure[p]++;
				}
			});

			routine(prim);

			Assert.Equal(expectedTotal, actualTotal);
			Assert.Equal(expectedPure, actualPure);
		}
	}
}
