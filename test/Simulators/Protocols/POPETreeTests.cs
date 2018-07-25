using System;
using System.Linq;
using Simulation.Protocol;
using ORESchemes.Shared;
using Xunit;
using Simulation.Protocol.POPE;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives.PRG;

namespace Test.Simulators.Protocols
{
	[Trait("Category", "Unit")]
	public class POPETree
	{
		private enum Origin
		{
			Left = 0b00,
			None = 0b01,
			Right = 0b11
		}

		private class FakeCipher : IGetSize
		{
			public int value;
			public int nonce;
			public Origin origin;

			public int GetSize() => 0;

			public long OrderValue
			{
				get
				{
					return (long)value * (long)Int32.MaxValue + ((long)origin * (long)(Int32.MaxValue / 4)) + (long)nonce;
				}
			}

			public FakeCipher(int value, int nonce, Origin origin)
			{
				if (nonce >= Int32.MaxValue / 4 || nonce < 0)
				{
					throw new ArgumentException("Nonce must be from 0 to Int32.MaxValue / 4 = {Int32.MaxValue / 4}");
				}

				this.value = value;
				this.nonce = nonce;
				this.origin = origin;
			}
		}

		private static int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly IPRG GTree;

		private List<FakeCipher> _fakeWorkingSet;

		private readonly Func<FakeCipher, long> _decode = cipher => cipher == null ? long.MaxValue : cipher.OrderValue;
		private readonly Func<FakeCipher, int> _index;

		public POPETree()
		{
			GTree = new DefaultPRGFactory(G.GetBytes(128 / 8)).GetPrimitive();

			_index = cipher =>
			{
				for (int j = 0; j < _fakeWorkingSet.Count; j++)
				{
					if (_decode(cipher) <= _decode(_fakeWorkingSet[j]))
					{
						return j;
					}
				}
				throw new InvalidOperationException();
			};
		}

		private Tree<FakeCipher> GetTree(int l)
		{
			return new Tree<FakeCipher>(
				new Options<FakeCipher>
				{
					L = 3,
					SetList =
						list => _fakeWorkingSet = list.OrderBy(c => _decode(c)).ToList(),
					GetSortedList =
						() => _fakeWorkingSet,
					IndexToInsert = _index,
					IndexOfResult = _index,
					G = GTree
				}
			);
		}


		[Fact]
		public void InsertionCorrectness()
		{
			var input = Enumerable
				.Range(1, 10)
				.Select(a => Enumerable.Repeat(a, 2))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a =>
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					return new EncryptedRecord<FakeCipher>
					{
						cipher = new FakeCipher(a, nonce, Origin.None),
						value = $"{a}-{nonce}"
					};
				})
				.ToList();

			var tree = GetTree(3);

			foreach (var item in input)
			{
				tree.Insert(item);
			}

			Assert.True(tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));
		}

		[Fact]
		public void FirstQueryCorrectness()
		{
			var input = Enumerable
				.Range(1, 10)
				.Select(a => Enumerable.Repeat(a, 2))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a =>
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					return new EncryptedRecord<FakeCipher>
					{
						cipher = new FakeCipher(a, nonce, Origin.None),
						value = $"{a}-{nonce}"
					};
				})
				.ToList();

			var tree = GetTree(3);

			foreach (var item in input)
			{
				tree.Insert(item);
			}

			var result = tree.Search(
				new FakeCipher(3, G.Next(Int32.MaxValue / 4), Origin.Left),
				new FakeCipher(7, G.Next(Int32.MaxValue / 4), Origin.Right)
			).ToHashSet();

			var expected = input
				.OrderBy(c => c.cipher.value)
				.SkipWhile(c => c.cipher.value < 3)
				.TakeWhile(c => c.cipher.value <= 7)
				.Select(c => c.value)
				.ToHashSet();

			Assert.Superset(expected, result);

			Assert.True(tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));

			tree.Validate(_decode);
		}

		[Theory]
		[InlineData(10, 2, 3, false)]
		[InlineData(100, 3, 3, false)]
		[InlineData(1000, 5, 3, false)]
		[InlineData(1000, 3, 5, false)]
		[InlineData(1000, 3, 10, false)]
		[InlineData(1000, 3, 20, false)]
		[InlineData(1000, 5, 10, true)]
		[InlineData(1000, 3, 50, false)]
		public void ManyQueriesCorrectness(int distinct, int duplicates, int l, bool insert)
		{
			const int RUNS = 100;

			var input = Enumerable
				.Range(1, distinct)
				.Select(a => Enumerable.Repeat(a, duplicates))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a =>
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					return new EncryptedRecord<FakeCipher>
					{
						cipher = new FakeCipher(a, nonce, Origin.None),
						value = $"{a}-{nonce}"
					};
				})
				.ToList();

			var tree = GetTree(l);

			foreach (var item in input)
			{
				tree.Insert(item);
			}

			for (int i = 0; i < RUNS; i++)
			{
				if (insert && i % 5 == 0)
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					var a = G.Next(1, distinct);

					var cipher = new EncryptedRecord<FakeCipher>
					{
						cipher = new FakeCipher(a, nonce, Origin.None),
						value = $"{a}-{nonce}"
					};

					input.Add(cipher);

					tree.Insert(cipher);

					Assert.True(tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));

					tree.Validate(_decode);
				}

				var from = G.Next(1, distinct);
				var to = G.Next(1, distinct);

				if (from > to)
				{
					var tmp = to;
					to = from;
					from = tmp;
				}

				var result = tree.Search(
					new FakeCipher(from, G.Next(Int32.MaxValue / 4), Origin.Left),
					new FakeCipher(to, G.Next(Int32.MaxValue / 4), Origin.Right)
				).ToHashSet();

				var expected = input
					.OrderBy(c => c.cipher.value)
					.SkipWhile(c => c.cipher.value < from)
					.TakeWhile(c => c.cipher.value <= to)
					.Select(c => c.value)
					.ToHashSet();

				Assert.Superset(expected, result);

				Assert.True(tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));

				tree.Validate(_decode);
			}
		}
	}
}
