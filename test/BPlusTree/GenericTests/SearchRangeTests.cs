using System;
using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C, K>
	{
		[Fact]
		public void SearchRangeNotExistingTest()
		{
			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryRange(_scheme.Encrypt(5, _key), _scheme.Encrypt(10, _key), null);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeEmptyTreeTest()
		{
			var result = new Tree<string, C>(
				_defaultOptions
			).TryRange(_scheme.Encrypt(2, _key), _scheme.Encrypt(3, _key), null);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeSingleElementTest()
		{
			List<string> output = new List<string>();

			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryRange(_scheme.Encrypt(2, _key), _scheme.Encrypt(4, _key), output);

			Assert.True(result);

			Assert.Equal(new List<string> { 3.ToString() }, output);
		}

		[Theory]
		[InlineData(58, 6545)]
		[InlineData(5000, 5050)]
		[InlineData(3000, 7000)]
		[InlineData(1, 10000)]
		public void SearchRangeMultilevelTest(int from, int to)
		{
			List<string> output = new List<string>();
			const int salt = 123;
			const int originalMax = 10000;

			from = (int)Math.Round(((double)from / originalMax) * _max);
			to = (int)Math.Round(((double)to / originalMax) * _max);

			if (from == to)
			{
				to++;
			}

			if (from == 0)
			{
				from++;
			}

			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, _max)
					.Select(val => val * salt)
					.ToList(),
				false,
				false
			).TryRange(_scheme.Encrypt(from * salt, _key), _scheme.Encrypt(to * salt, _key), output);

			Assert.True(result);

			Assert.Equal(
				Enumerable
					.Range(from, to - from + 1)
					.Select(val => (val * salt).ToString())
					.OrderBy(val => val)
					.ToList(),
				output
					.OrderBy(val => val)
					.ToList()
			);
		}

		[Fact]
		public void SearchRangeNonExistingMultilevelTest()
		{
			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryRange(_scheme.Encrypt(101 * 101, _key), _scheme.Encrypt(120 * 120, _key), null);

			Assert.False(result);
		}

		[Theory]
		[InlineData(3, 2)]
		[InlineData(2, 2)]
		public void SearchRangeImproperTest(int start, int end)
		{
			var tree = new Tree<string, C>(_defaultOptions);

			var startCipher = _scheme.Encrypt(start, _key);
			var endCipher = _scheme.Encrypt(end, _key);

			var exception = Assert.Throws<ArgumentException>(
				() => tree.TryRange(startCipher, endCipher, null)
			);

			Assert.Equal("Improper range", exception.Message);
		}
	}
}
