using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.CJJJKRS;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using Xunit;
using static Test.ORESchemes.Primitives.EventsTestsShared;
using Scheme = ORESchemes.CJJJKRS.CJJJKRS<Test.ORESchemes.CJJJKRS.StringWord, Test.ORESchemes.CJJJKRS.NumericIndex>;

namespace Test.ORESchemes
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
					.Range(1, RUNS * 5)
					.ToDictionary(
						_ => new StringWord { Value = randomString(G.Next(5, 10)) },
						_ => Enumerable
							.Range(1, G.Next(1, RUNS / 5))
							.Select(__ => new NumericIndex { Value = G.Next() })
							.ToArray()
					);

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

		// [Fact]
		// public void NodeVisitedEvents()
		// {
		// 	var count = new Reference<int>();
		// 	count.Value = 0;

		// 	var database = _client.Setup(_input);
		// 	_server = new Scheme.Server(database);
		// 	_server.NodeVisited += new NodeVisitedEventHandler(_ => count.Value++);
		// 	_server.PageSize = 600;

		// 	var keyword = new StringWord { Value = "Dmytro" };
		// 	var token = _client.Trapdoor(keyword);
		// 	var encrypted = _server.Search(token);
		// 	_client.Decrypt(encrypted, keyword, e => new NumericIndex { Value = BitConverter.ToInt32(e, 0) });

		// 	Assert.NotEqual(0, count.Value);
		// }

		// [Fact]
		// public void NodeVisitedEventsNoPageSize()
		// {
		// 	var triggered = new Reference<bool>();
		// 	triggered.Value = false;

		// 	var database = _client.Setup(_input);
		// 	_server = new Scheme.Server(database);
		// 	_server.NodeVisited += new NodeVisitedEventHandler(_ => triggered.Value = true);

		// 	var keyword = new StringWord { Value = "Dmytro" };
		// 	var token = _client.Trapdoor(keyword);
		// 	var encrypted = _server.Search(token);
		// 	_client.Decrypt(encrypted, keyword, e => new NumericIndex { Value = BitConverter.ToInt32(e, 0) });

		// 	Assert.False(triggered.Value);
		// }

		[Fact]
		public void DatabaseSize()
		{
			(var database, var _) = _client.Setup(_input);

			// TODO
			// Assert.InRange(database.Size, 513 * 5 * 5, 513 * 5 * 5 * 2);
		}
	}
}
