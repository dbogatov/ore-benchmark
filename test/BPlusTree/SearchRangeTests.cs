using System;
using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;

namespace Test
{
	public partial class BPlusTreeTests
	{
		[Fact]
		public void SearchRangeNotExistingTest()
		{
			var result = ConstructTree(
				new Options<int, long>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3 }
			).TryRange(5, 10, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeEmptyTreeTest()
		{
			var result = new Tree<string, long, int>(
				new Options<int, long>(
					new NoEncryptionScheme(),
					3
				)
			).TryRange(2, 3, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeSingleElementTest()
		{
			List<string> output = null;

			var result = ConstructTree(
				new Options<int, long>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3 }
			).TryRange(2, 4, out output);

			Assert.True(result);

			Assert.Equal(new List<string> { 3.ToString() }, output);
		}

		[Theory]
		[InlineData(10000, 58, 6545)]
		[InlineData(10000, 5000, 5001)]
		[InlineData(10000, 3000, 7000)]
		[InlineData(10000, 1, 10000)]
		public void SearchRangeMultilevelTest(int max, int from, int to)
		{
			List<string> output = null;

			var result = ConstructTree(
				new Options<int, long>(
					new NoEncryptionScheme(),
					3
				),
				Enumerable
					.Range(1, max)
					.Select(val => val * 123)
					.ToList(),
				false,
				false
			).TryRange(from * 123, to * 123, out output);

			Assert.True(result);

			Assert.Equal(
				Enumerable
					.Range(from, to - from + 1)
					.Select(val => (val * 123).ToString())
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
				new Options<int, long>(
					new NoEncryptionScheme(),
					3
				),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryRange(101 * 101, 120 * 120, out _);

			Assert.False(result);
		}

		[Theory]
		[InlineData(3, 2)]
		[InlineData(2, 2)]
		public void SearchRangeEndBeforeStartTest(int start, int end)
		{
			Assert.Throws<ArgumentException>(
				() =>
				new Tree<string, long, int>(
					new Options<int, long>(
						new NoEncryptionScheme(),
						3
					)
				).TryRange(start, end, out _)
			);
		}
	}
}
