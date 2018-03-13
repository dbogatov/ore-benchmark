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
		public void SearchNotExistingElementTest()
		{
			var result = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3 }
			).TryGet(2, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchNotEmptyTreeTest()
		{
			var result = new Tree<string>(new Options()).TryGet(2, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchSingleElementTest()
		{
			var output = "";

			var result = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3 }
			).TryGet(3, out output);

			Assert.True(result);

			Assert.Equal(output, 3.ToString());
		}

		[Fact]
		public void SearchSingleElementMultilevelTest()
		{
			var output = "";

			var result = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(58*58, out output);

			Assert.True(result);

			Assert.Equal(output, (58*58).ToString());
		}

		[Fact]
		public void SearchNonExistingElementMultilevelTest()
		{
			var result = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(158, out _);

			Assert.False(result);
		}
	}
}
