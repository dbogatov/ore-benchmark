using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C, K>
	{
		[Fact]
		public void DuplicateInsertion()
		{
			ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3 }
			);
		}

		[Fact]
		public void DuplicateSearch()
		{
			var output = new List<string>();

			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			).TryGet(_scheme.Encrypt(3, _key), output);

			Assert.True(result);

			Assert.All(output, n => Assert.Equal(3.ToString(), n));
			Assert.Equal(3, output.Count);
		}

		[Fact]
		public void DuplicateSearchSingle()
		{
			var output = "";
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			);

			Assert.Throws<InvalidOperationException>(
				() => tree.TryGetSingle(_scheme.Encrypt(3, _key), out output)
			);
		}

		[Fact]
		public void DuplicateUpdate()
		{
			var output = new List<string>();

			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			);

			var result = tree.Update(_scheme.Encrypt(6, _key), "six");
			Assert.True(result);

			result = tree.TryGet(_scheme.Encrypt(6, _key), output);
			Assert.True(result);

			Assert.All(output, n => Assert.Equal("six", n));
			Assert.Equal(2, output.Count);
		}

		[Fact]
		public void DuplicateUpdateSingle()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			);

			Assert.Throws<InvalidOperationException>(
				() => tree.UpdateSingle(_scheme.Encrypt(6, _key), "")
			);
		}

		[Fact]
		public void DuplicateRangeQuery()
		{
			var output = new List<string>();

			var result = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			).TryRange(
				_scheme.Encrypt(5, _key),
				_scheme.Encrypt(6, _key),
				output
			);

			Assert.True(result);
			Assert.Equal(new List<int> { 5, 6, 6 }.Select(n => n.ToString()), output);
		}

		[Fact]
		public void DuplicateDelete()
		{
			var output = new List<string>();

			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 }
			);
			
			var result = tree.Delete(_scheme.Encrypt(6, _key));
			Assert.True(result);

			Assert.False(tree.TryGet(_scheme.Encrypt(6, _key), null));
		}
	}
}
