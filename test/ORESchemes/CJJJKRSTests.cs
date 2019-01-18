using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crypto.CJJJKRS;
using Crypto.Shared;
using Crypto.Shared.Primitives;
using Xunit;
using static Test.Crypto.Primitives.EventsTestsShared;
using Scheme = Crypto.CJJJKRS.Scheme<Test.Crypto.CJJJKRS.StringWord, Test.Crypto.CJJJKRS.NumericIndex>;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class CJJJKRS
	{
		public class StringWord : IWord
		{
			public string Value { get; set; }

			public byte[] ToBytes() => Encoding.Default.GetBytes(Value);

			public override int GetHashCode() => Value.GetHashCode();
		}

		public class NumericIndex : IIndex
		{
			public int Value { get; set; }

			public byte[] ToBytes() => BitConverter.GetBytes(Value);

			public override bool Equals(object obj) => Value.Equals(obj);

			public override int GetHashCode() => Value.GetHashCode();

			static public NumericIndex Decode(byte[] encoded) => new NumericIndex { Value = BitConverter.ToInt32(encoded, 0) };
		}

		private readonly Scheme.Client _client;
		private Scheme.Server _server;

		static readonly int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly int RUNS = 50;
		private readonly Dictionary<StringWord, NumericIndex[]> _input =
			new Dictionary<StringWord, NumericIndex[]> {
				{
					new StringWord { Value = "Dmytro" },
					new NumericIndex[] {
						new NumericIndex { Value = 21 },
						new NumericIndex { Value = 05 }
					}
				},
				{
					new StringWord { Value = "Alex" },
					new NumericIndex[] {
						new NumericIndex { Value = 26 },
						new NumericIndex { Value = 10 }
					}
				}
			};

		public CJJJKRS()
		{
			_client = new Scheme.Client(G.GetBytes(128 / 8));
		}

		public IIndex[] PrimitiveRun(string word)
		{
			(var database, var key) = _client.Setup(_input);

			_server = new Scheme.Server(database);

			// Search protocol
			var keyword = new StringWord { Value = word };
			var token = _client.Trapdoor(keyword, key);
			return _server.Search(token, NumericIndex.Decode);
		}

		private bool OutputAsExpected(int[] expected, IIndex[] actual)
		{
			if (expected.Count() != actual.Count())
			{
				return false;
			}

			var expectedSorted = expected.OrderBy(e => e);
			var actualSorted = actual.Select(e => ((NumericIndex)e).Value).OrderBy(e => e);

			return
				expectedSorted
					.Zip(actualSorted, (a, b) => a == b)
					.All(e => e);
		}

		Dictionary<StringWord, NumericIndex[]> GenerateInput(int keywords, (int from, int to) indices)
		{
			string RandomString(int length)
			{
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				return new string(
					Enumerable
						.Repeat(chars, length)
						.Select(s => s[G.Next(s.Length)])
						.ToArray()
				);
			}

			return
				Enumerable
					.Range(1, keywords)
					.ToDictionary(
						_ => new StringWord { Value = RandomString(G.Next(5, 10)) },
						_ => Enumerable
							.Range(1, G.Next(indices.from, indices.to))
							.Select(__ => new NumericIndex { Value = G.Next() })
							.ToArray()
					);
		}

		[Fact]
		public void NoExceptions() => PrimitiveRun("Dmytro");

		[Fact]
		public void PrimitiveCorrectness()
		{
			var result = PrimitiveRun("Dmytro");

			Assert.True(OutputAsExpected(new int[] { 21, 05 }, result));
		}

		[Fact]
		public void Correctness()
		{
			for (int i = 0; i < RUNS; i++)
			{
				var input = GenerateInput(RUNS * 5, (1, RUNS / 5));

				(var database, var key) = _client.Setup(input);

				_server = new Scheme.Server(database);

				foreach (var keywordIndices in input)
				{
					var keyword = keywordIndices.Key;

					var token = _client.Trapdoor(keyword, key);

					var result = _server.Search(token, NumericIndex.Decode);

					Assert.True(OutputAsExpected(
						input[keywordIndices.Key].Cast<NumericIndex>().Select(e => e.Value).ToArray(),
						result
					));
				}
			}
		}

		[Fact]
		public void NonExistentKeyword()
		{
			(var database, var key) = _client.Setup(_input);

			_server = new Scheme.Server(database);

			// Search protocol
			var keyword = new StringWord { Value = "NonExistent" };
			var token = _client.Trapdoor(keyword, key);
			var result = _server.Search(token, NumericIndex.Decode);

			Assert.Empty(result);
		}

		[Fact]
		public void PrimitiveEvents()
		{
			var expectedTotal =
				new Dictionary<Primitive, int> {
					{ Primitive.PRG, 1 },
					{ Primitive.PRF, 13 },
					{ Primitive.AES, 20 },
					{ Primitive.Symmetric, 6 }
				};
			var expectedPure =
				new Dictionary<Primitive, int> {
					{ Primitive.PRG, 1 },
					{ Primitive.PRF, 13 },
					{ Primitive.Symmetric, 6 }
				};

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

			var handler = new PrimitiveUsageEventHandler((p, impure) =>
			{
				actualTotal[p]++;
				if (!impure)
				{
					actualPure[p]++;
				}
			});
			_client.PrimitiveUsed += handler;

			(var database, var key) = _client.Setup(_input);
			_server = new Scheme.Server(database);

			_server.PrimitiveUsed += handler;

			// Search protocol
			var keyword = new StringWord { Value = "Dmytro" };
			var token = _client.Trapdoor(keyword, key);
			_server.Search(token, NumericIndex.Decode);

			Assert.Equal(expectedTotal, actualTotal);
			Assert.Equal(expectedPure, actualPure);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void NodeVisitedEvents(bool pack)
		{
			var b = pack ? 10 : int.MaxValue;
			var B = pack ? 20 : 1;
			var expected = pack ? 5 : 100;

			var count = new Reference<int>();
			count.Value = 0;

			var input = GenerateInput(50, (100, 100));

			(var database, var key) = _client.Setup(input, b, B);
			_server = new Scheme.Server(database, G.GetBytes(128 / 8));
			_server.NodeVisited += new NodeVisitedEventHandler(_ => count.Value++);
			_server.PageSize = 20 * sizeof(int) * 8; // 20 indices

			var keyword = input.Keys.First();
			var token = _client.Trapdoor(keyword, key);
			_server.Search(token, NumericIndex.Decode, b, B);

			Assert.Equal(expected, count.Value);
		}

		[Fact]
		public void NodeVisitedEventsNoPageSize()
		{
			var triggered = new Reference<bool>();
			triggered.Value = false;

			(var database, var key) = _client.Setup(_input);
			_server = new Scheme.Server(database);
			_server.NodeVisited += new NodeVisitedEventHandler(_ => triggered.Value = true);

			var keyword = new StringWord { Value = "Dmytro" };
			var token = _client.Trapdoor(keyword, key);
			var encrypted = _server.Search(token, NumericIndex.Decode);

			Assert.False(triggered.Value);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void DatabaseSize(bool pack)
		{
			var b = pack ? 10 : int.MaxValue;
			var B = pack ? 20 : 1;
			var expected = 50 *
				(
					pack ?
					128 * 4 + 4 * (128 * (20 * 4 * 8 / 128) + 128) :
					61 * 128 + 61 * (128 + 128)
				);

			var input = GenerateInput(50, (61, 61));

			(var database, var _) = _client.Setup(input, b, B);

			Assert.Equal(expected, database.Size);
		}
	}
}
