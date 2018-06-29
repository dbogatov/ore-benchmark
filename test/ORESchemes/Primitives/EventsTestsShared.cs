using Xunit;
using ORESchemes.Shared.Primitives;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Test.ORESchemes.Primitives
{
	public static class EventsTestsShared
	{
		/// <summary>
		/// Runs primitive events test for given parameters
		/// </summary>
		/// <param name="prim">The instance of primitive to test</param>
		/// <param name="routine">An action that runs the primitive (taken as parameter)</param>
		/// <param name="expectedTotal">The dictionary with expected total primitive usage</param>
		/// <param name="expectedPure">The dictionary with expected pure primitive usage</param>
		/// <typeparam name="T">Type of the primitive</typeparam>
		public static void Events<T>(
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
