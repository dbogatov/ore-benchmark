using System;
using Xunit;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C, K>
	{
		[Fact]
		public void SearchNotExistingElementTest()
		{
			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryGet(_scheme.Encrypt(2, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchEmptyTreeTest()
		{
			var result = new Tree<string, C>(
				_defaultOptions
			).TryGet(_scheme.Encrypt(2, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchSingleElementTest()
		{
			var output = "";

			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			).TryGet(_scheme.Encrypt(3, _key), out output);

			Assert.True(result);

			Assert.Equal(3.ToString(), output);
		}

		[Fact]
		public void SearchSingleElementMultilevelTest()
		{
			var output = "";

			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(_scheme.Encrypt(58 * 58, _key), out output);

			Assert.True(result);

			Assert.Equal((58 * 58).ToString(), output);
		}

		[Fact]
		public void SearchNonExistingElementMultilevelTest()
		{
			var result = ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryGet(_scheme.Encrypt(158, _key), out _);

			Assert.False(result);
		}
	}
}
