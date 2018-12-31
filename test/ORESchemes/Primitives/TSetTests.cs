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
using ORESchemes.Shared.Primitives;

namespace Test.ORESchemes.Primitives.TSet
{
	[Trait("Category", "Unit")]
	public class CashTSet
	{
		private readonly ITSet T;
		private readonly IPRF F;
		static readonly int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly int RUNS = 50;
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

		private BitArray[] RunPipeline(Dictionary<IWord, BitArray[]> input, IWord keyword)
		{
			(var TSet, var key) = T.Setup(input);

			var stag = T.GetTag(key, keyword);

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
		public void NoExceptions() => RunPipeline(_sampleInput, new StringWord { Value = "Dmytro" });

		[Fact]
		public void CorrectnessSimple()
		{
			var output = RunPipeline(_sampleInput, new StringWord { Value = "Dmytro" });

			Assert.True(OutputAsExpected(
				new BitArray[] {
					new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("WPI"))),
					new BitArray(F.PRF(_prfKey, Encoding.Default.GetBytes("BU")))
				},
				output
			));
		}

		[Fact]
		public void Correctness()
		{
			string randomString(int length)
			{
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				return new string(
					Enumerable
						.Repeat(chars, length)
						.Select(s => s[G.Next(s.Length)])
						.ToArray()
				);
			}

			for (int i = 0; i < RUNS; i++)
			{
				var input = Enumerable
					.Range(1, RUNS * 10)
					.ToDictionary(
						_ => (IWord)new StringWord { Value = randomString(G.Next(5, 16)) },
						_ => Enumerable
							.Range(1, RUNS / 10)
							.Select(
								__ => new BitArray(
									F.PRF(
										_prfKey,
										Encoding.Default.GetBytes(
											randomString(G.Next(5, 16))
										)
									)
								)
							)
							.ToArray()
					);

				(var TSet, var key) = T.Setup(input);

				foreach (var keywordIndices in input)
				{
					var stag = T.GetTag(key, keywordIndices.Key);

					var output = T.Retrive(TSet, stag);

					Assert.True(OutputAsExpected(
						input[keywordIndices.Key],
						output
					));
				}
			}
		}

		[Fact]
		public void MalformedWord()
		{
			Assert.Throws<InvalidOperationException>(
				() => RunPipeline(_sampleInput, new StringWord { Value = "Malformed" })
			);
		}

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<ITSet>(
				T,
				set =>
				{
					(var TSet, var key) = set.Setup(_sampleInput);

					var stag = set.GetTag(key, new StringWord { Value = "Dmytro" });

					set.Retrive(TSet, stag);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.TSet, 3 },
					{ Primitive.PRG, 5 },
					{ Primitive.PRF, 3 },
					{ Primitive.AES, 8 },
					{ Primitive.Hash, 6 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.TSet, 3 }
				}
			);
		}
	}
}
