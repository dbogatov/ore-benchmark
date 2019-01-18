using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crypto.Shared.Primitives.PRF;
using Crypto.Shared.Primitives.TSet;
using Crypto.Shared;
using Xunit;
using System.Numerics;
using Crypto.Shared.Primitives;
using Crypto.Shared.Primitives.Symmetric;
using static Test.Crypto.Primitives.EventsTestsShared;

namespace Test.Crypto.Primitives.TSet
{
	[Trait("Category", "Unit")]
	public class CashTSet128 : CashTSet
	{
		public CashTSet128() : base(alpha: 128) { }
	}

	[Trait("Category", "Unit")]
	public class CashTSet256 : CashTSet
	{
		public CashTSet256() : base(alpha: 128) { }
	}

	[Trait("Category", "Unit")]
	public class CashTSet1024 : CashTSet
	{
		public CashTSet1024() : base(alpha: 1024) { }
	}

	[Trait("Category", "Unit")]
	public class BitArrayChecks
	{
		[Fact]
		public void BitArrayIsEqualTo()
		{
			var G = new Random(123456);
			byte[] seedBytes = new byte[128 / 8];
			G.NextBytes(seedBytes);
			var seedBits = new BitArray(seedBytes);

			var first = new BitArray(seedBits);
			var second = new BitArray(seedBits);

			var third = new BitArray(seedBits);
			third[65] = !third[65];

			var fourth = new BitArray(seedBits);
			fourth = new BitArray(fourth.Prepend(new BitArray(new bool[] { false })));

			Assert.True(first.IsEqualTo(second));
			Assert.False(first.IsEqualTo(third));
			Assert.False(first.IsEqualTo(fourth));
		}

		[Fact]
		public void BitArrayToBytes()
		{
			var G = new Random(123456);
			byte[] bytes = new byte[128 / 8];
			G.NextBytes(bytes);

			var bits = new BitArray(bytes);
			var shorter = new BitArray(64);

			Assert.Equal(bytes, bits.ToBytes());
			Assert.NotEqual(bytes, shorter.ToBytes());
		}
	}

	public abstract class CashTSet
	{
		private readonly ITSet T;
		private readonly IPRF F;
		private readonly ISymmetric E;

		static readonly int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly int RUNS = 20;
		private readonly byte[] _encKey = new byte[128 / 8];
		private readonly double _p = 0.1; // for tests, trade space for speed

		private readonly Dictionary<IWord, BitArray[]> _sampleInput;

		private readonly int _alpha;

		public CashTSet(int alpha)
		{
			_alpha = alpha;

			RUNS = (int)Math.Round(50 * 128 / (1.0 * _alpha));
			RUNS = Math.Max(RUNS, 10);
			RUNS = Math.Min(RUNS, 50);

			byte[] entropy = new byte[128 / 8];
			G.NextBytes(entropy);
			G.NextBytes(_encKey);

			F = new PRFFactory().GetPrimitive();
			E = new SymmetricFactory().GetPrimitive();

			T = new global::Crypto.Shared.Primitives.TSet.CashTSet(entropy);

			_sampleInput = new Dictionary<IWord, BitArray[]> {
				{
					new StringWord { Value = "Dmytro" },
					new BitArray[] {
						EncryptForTSet("WPI"),
						EncryptForTSet("BU")
					}
				},
				{
					new StringWord { Value = "Alex" },
					new BitArray[] {
						EncryptForTSet("KPI"),
						EncryptForTSet("WPI")
					}
				}
			};
		}

		private BitArray EncryptForTSet(string input)
		{
			var bytesIn = Encoding.Default.GetBytes(input);
			byte[] bytesOut;

			switch (_alpha)
			{
				case 128:
					bytesOut = F.PRF(_encKey, bytesIn);
					break;
				case 256:
					bytesOut = E.Encrypt(_encKey, bytesIn);
					break;
				case var _ when _alpha > 256 && _alpha % 8 == 0:
					bytesOut = E.Encrypt(_encKey, bytesIn).Concat(new byte[(_alpha - 256) / 8]).ToArray();
					break;
				default:
					throw new ArgumentException($"ALPHA = {_alpha} is not allowed.");
			}

			return new BitArray(bytesOut);
		}

		private BitArray[] RunPipeline(Dictionary<IWord, BitArray[]> input, IWord keyword)
		{
			(var TSet, var key) = T.Setup(input, _p);

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
				_sampleInput.Where(v => ((StringWord)v.Key).Value == "Dmytro").First().Value,
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
								__ => EncryptForTSet(randomString(G.Next(5, 16)))
							)
							.ToArray()
					);

				(var TSet, var key) = T.Setup(input, _p);

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
		public void NonExistentWord()
		{
			(var TSet, var key) = T.Setup(_sampleInput, _p);

			var stag = T.GetTag(key, new StringWord { Value = "NonExistent" });

			var empty = T.Retrive(TSet, stag);

			Assert.Empty(empty);
		}

		[Fact]
		public void UnequalStrings()
		{
			_sampleInput.First().Value[0] = new BitArray(_alpha + 5);
			Assert.Throws<ArgumentException>(
				() => RunPipeline(_sampleInput, new StringWord { Value = "Dmytro" })
			);
		}

		[Fact]
		public void PrimitiveEvents()
		{
			EventsTestsShared.Events<ITSet>(
				T,
				set =>
				{
					(var TSet, var key) = set.Setup(_sampleInput, _p);

					var stag = set.GetTag(key, new StringWord { Value = "Dmytro" });

					set.Retrive(TSet, stag);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.TSet, 3 },
					{ Primitive.PRG, 97 },
					{ Primitive.PRF, 3 },
					{ Primitive.AES, 100 },
					{ Primitive.Hash, 6 * (_alpha / 128) }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.TSet, 3 }
				}
			);
		}

		[Fact]
		public void NodeVisitedEventsForNoPageSize()
		{
			var triggered = new Reference<bool>();
			triggered.Value = false;

			T.NodeVisited += new NodeVisitedEventHandler(_ => triggered.Value = true);

			(var TSet, var key) = T.Setup(_sampleInput, _p);
			var stag = T.GetTag(key, new StringWord { Value = "Dmytro" });

			T.Retrive(TSet, stag);

			Assert.False(triggered.Value);
		}

		[Fact]
		public void NodeVisitedEvents()
		{
			var count = new Reference<int>();
			count.Value = 0;

			T.NodeVisited += new NodeVisitedEventHandler(_ => count.Value++);
			T.PageSize = _alpha * 3;

			(var TSet, var key) = T.Setup(_sampleInput, _p);
			var stag = T.GetTag(key, new StringWord { Value = "Dmytro" });

			T.Retrive(TSet, stag);

			Assert.NotEqual(0, count.Value);
		}

		[Fact]
		/// <summary>
		/// This one actually tests a bug fix
		/// </summary>
		public void DuplicateQuery()
		{
			(var TSet, var key) = T.Setup(_sampleInput, _p);
			var keyword = new StringWord { Value = "Dmytro" };

			var stag = T.GetTag(key, keyword);

			var first = T.Retrive(TSet, stag);
			var second = T.Retrive(TSet, stag);

			Assert.True(OutputAsExpected(
				_sampleInput[keyword],
				first
			));

			Assert.True(OutputAsExpected(
				_sampleInput[keyword],
				second
			));
		}
	}
}
