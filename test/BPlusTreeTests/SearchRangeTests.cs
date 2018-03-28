using System;
using Xunit;
using DataStructures.BPlusTree;
using OPESchemes;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	public partial class BPlusTreeTests
	{
		[Fact]
		public void SearchRangeNotExistingTest()
		{
			var result = ConstructTree(
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
					3
				),
				new List<int> { 3 }
			).TryRange(5, 10, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeEmptyTreeTest()
		{
			var result = new Tree<string, int, int>(
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
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
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
					3
				),
				new List<int> { 3 }
			).TryRange(2, 4, out output);

			Assert.True(result);

			Assert.Equal(new List<string> { 3.ToString() }, output);
		}

		[Fact]
		public void SearchRangeMultilevelTest()
		{
			List<string> output = null;

			var result = ConstructTree(
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
					3
				),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryRange(58 * 58, 65 * 65, out output);

			Assert.True(result);

			Assert.Equal(
				Enumerable
					.Range(58, 65 - 58 + 1)
					.Select(val => (val * val).ToString())
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
				new Options<int, int>(
					OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
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
				new Tree<string, int, int>(
					new Options<int, int>(
						OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
						3
					)
				).TryRange(start, end, out _)
			);
		}
	}
}
