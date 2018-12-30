using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.TSet;
using Xunit;

namespace Test.ORESchemes.Primitives.TSet
{
	[Trait("Category", "Unit")]
	public class CashTSet
	{
		private readonly ITSet T;
		private readonly IPRF F;
		static readonly int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly int RUNS = 1000;
		private readonly byte[] _prfKey = new byte[128 / 8];

		public CashTSet()
		{
			byte[] entropy = new byte[128 / 8];
			G.NextBytes(entropy);
			G.NextBytes(_prfKey);
			
			F = new PRFFactory().GetPrimitive();

			T = new global::ORESchemes.Shared.Primitives.TSet.CashTSet(entropy);
		}

		// [Fact]
		public void NoExceptions()
		{
			var input = new Dictionary<IWord, BitArray[]> {
				{
					new StringWord { Value = "Dmytro" },
					new BitArray[] {
						new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("WPI"))),
						new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("BU")))
					}
				},
				{
					new StringWord { Value = "Alex" },
					new BitArray[] {
						new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("KPI"))),
						new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("WPI")))
					}
				}
			};
			
			(var TSet, var key) = T.Setup(input);
			
			var stag = T.GetTag(key, new StringWord { Value = "Dmytro" });
			
			var t = T.Retrive(TSet, stag);
		}

		// [Fact]
		// public void Events()
		// {
		// 	EventsTestsShared.Events<IPPH>(
		// 		R,
		// 		pph =>
		// 		{
		// 			var key = pph.KeyGen();
		// 			pph.Test(
		// 				key.testKey,
		// 				pph.Hash(key.hashKey, new byte[] { }),
		// 				pph.Hash(key.hashKey, new byte[] { })
		// 			);
		// 		},
		// 		new Dictionary<Primitive, int> {
		// 			{ Primitive.PPH, 3 },
		// 			{ Primitive.PRG, 2 },
		// 			{ Primitive.AES, 2 }
		// 		},
		// 		new Dictionary<Primitive, int> {
		// 			{ Primitive.PPH, 3 }
		// 		}
		// 	);
		// }
	}
}
