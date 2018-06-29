using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
	{
		[Fact]
		public void SearchNotExistingElement()
		{
			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryGetSingle(_scheme.Encrypt(2, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchEmptyTree()
		{
			var result = new Tree<string, C>(
				_defaultOptions
			).TryGetSingle(_scheme.Encrypt(2, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchSingleElement()
		{
			var output = "";

			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryGetSingle(_scheme.Encrypt(3, _key), out output);

			Assert.True(result);

			Assert.Equal(3.ToString(), output);
		}

		[Fact]
		public void SearchSingleElementMultilevel()
		{
			var output = "";

			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGetSingle(_scheme.Encrypt(58 * 58, _key), out output);

			Assert.True(result);

			Assert.Equal((58 * 58).ToString(), output);
		}

		[Fact]
		public void SearchNonExistingElementMultilevel()
		{
			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGetSingle(_scheme.Encrypt(158, _key), out _);

			Assert.False(result);
		}
	}
}
