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
				this.value = value;
				this.nonce = nonce;
				this.origin = origin;
			}
		}

		private static int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly IPRG GTree;

		private readonly Tree<FakeCipher> _tree;
		private List<FakeCipher> _fakeWorkingSet;

		private readonly Func<FakeCipher, long> _decode = cipher => cipher == null ? long.MaxValue : cipher.OrderValue;

		public POPETree()
		{
			GTree = new DefaultPRGFactory(G.GetBytes(128 / 8)).GetPrimitive();



			Func<FakeCipher, int> index = cipher =>
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

			_tree = new Tree<FakeCipher>(
				new Options<FakeCipher>
				{
					L = 3,
					SetList =
						list => _fakeWorkingSet = list.OrderBy(c => _decode(c)).ToList(),
					GetSortedList =
						() => _fakeWorkingSet,
					IndexToInsert = index,
					IndexOfResult = index,
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

			foreach (var item in input)
			{
				_tree.Insert(item);
			}

			Assert.True(_tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));
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

			foreach (var item in input)
			{
				_tree.Insert(item);
			}

			var result = _tree.Search(
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

			Assert.True(_tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));

			_tree.Validate(_decode);
		}

		[Fact]
		public void ManyQueriesCorrectness()
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

			foreach (var item in input)
			{
				_tree.Insert(item);
			}

			for (int i = 0; i < 100; i++)
			{
				var from = G.Next(1, 10);
				var to = G.Next(1, 10);

				if (from > to)
				{
					var tmp = to;
					to = from;
					from = tmp;
				}

				if (i == 89)
				{
					
				}

				var result = _tree.Search(
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

				Assert.True(_tree.ValidateElementsInserted(input.Select(c => _decode(c.cipher)).ToList(), _decode));

				_tree.Validate(_decode);
			}
		}
	}
}
