using System;
using Xunit;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
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
				new Options<long>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3 }
			).TryGet(2, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchEmptyTreeTest()
		{
			var result = new Tree<string, long>(
				new Options<long>(
					new NoEncryptionScheme(),
					3
				)
			).TryGet(2, out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchSingleElementTest()
		{
			var output = "";

			var result = ConstructTree(
				new Options<long>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3 }
			).TryGet(3, out output);

			Assert.True(result);

			Assert.Equal(3.ToString(), output);
		}

		[Fact]
		public void SearchSingleElementMultilevelTest()
		{
			var output = "";

			var result = ConstructTree(
				new Options<long>(
					new NoEncryptionScheme(),
					3
				),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(58*58, out output);

			Assert.True(result);

			Assert.Equal((58*58).ToString(), output);
		}

		[Fact]
		public void SearchNonExistingElementMultilevelTest()
		{
			var result = ConstructTree(
				new Options<long>(
					new NoEncryptionScheme(),
					3
				),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(158, out _);

			Assert.False(result);
		}
	}
}
