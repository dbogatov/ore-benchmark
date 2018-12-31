using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.TSet;
using ORESchemes.Shared;
using Xunit;
using System.Numerics;

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

		private readonly Dictionary<IWord, BitArray[]> _sampleInput;

		public CashTSet()
		{
			byte[] entropy = new byte[128 / 8];
			G.NextBytes(entropy);
			G.NextBytes(_prfKey);

			F = new PRFFactory().GetPrimitive();

			T = new global::ORESchemes.Shared.Primitives.TSet.CashTSet(entropy);

			_sampleInput = new Dictionary<IWord, BitArray[]> {
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
		}

		[Fact]
		public void BitArrayIsEqualTo()
		{
			byte[] seedBytes = new byte[128 / 8];
			G.NextBytes(seedBytes);
			var seedBits = new BitArray(seedBytes);

			var first = new BitArray(seedBits);
			var second = new BitArray(seedBits);

			var third = new BitArray(seedBits);
			third[65] = !third[65];

			var fourth = new BitArray(seedBits);
			fourth = fourth.Prepend(new BitArray(new bool[] { false }));

			Assert.True(first.IsEqualTo(second));
			Assert.False(first.IsEqualTo(third));
			Assert.False(first.IsEqualTo(fourth));
		}

		private BitArray[] RunPipeline(Dictionary<IWord, BitArray[]> input, string keyword)
		{
			(var TSet, var key) = T.Setup(input);

			var stag = T.GetTag(key, new StringWord { Value = keyword });

			return T.Retrive(TSet, stag);
		}

		private bool OutputAsExpected(BitArray[] expected, BitArray[] actual)
		{
			if (expected.Count() != actual.Count())
			{
				return false;
			}

			var expectedSorted = expected.OrderBy(e => new BigInteger(e.ToBytes()));
			var actualSorted = actual.OrderBy(e => new BigInteger(e.ToBytes()));

			return
				expectedSorted
					.Zip(actualSorted, (a, b) => a.IsEqualTo(b))
					.All(e => e);
		}

		[Fact]
		public void NoExceptions() => RunPipeline(_sampleInput, "Dmytro");

		[Fact]
		public void CorrectnessSimple()
		{
			var output = RunPipeline(_sampleInput, "Dmytro");

			Assert.True(OutputAsExpected(
				new BitArray[] {
					new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("WPI"))),
					new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("BU")))
				},
				output
			));
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
