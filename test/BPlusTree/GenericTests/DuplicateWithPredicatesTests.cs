using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.BPlusTree;
using Xunit;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
	{
		private Tree<string, C> DuplicatesTree()
			=> ConstructTree(
				_defaultOptions,
				new List<int> { 3, 3, 3, 5, 6, 6 },
				data: new List<string> { "three az", "three bz", "three cy", "five d", "six e", "six d" }
			);

		[Fact]
		public void DuplicatePredicateSearch()
		{
			var output = new List<string>();

			var result = DuplicatesTree().TryGet(_scheme.Encrypt(3, _key), output, s => s.Contains('b'));

			Assert.True(result);

			Assert.Single(output);
			Assert.Equal("three bz", output.Single());
		}

		[Fact]
		public void DuplicatePredicateSearchSingle()
		{
			var output = "";
			var tree = DuplicatesTree();

			Assert.Throws<InvalidOperationException>(
				() => tree.TryGetSingle(_scheme.Encrypt(3, _key), out output, s => s.Contains('z'))
			);
		}

		[Fact]
		public void DuplicatePredicateUpdate()
		{
			var output = new List<string>();

			var tree = DuplicatesTree();

			var result = tree.Update(_scheme.Encrypt(6, _key), "six o", s => s.Contains('d'));
			Assert.True(result);

			result = tree.TryGet(_scheme.Encrypt(6, _key), output);
			Assert.True(result);

			Assert.Equal(new List<string> { "six e", "six o" }, output);
		}

		[Fact]
		public void DuplicatePredicateUpdateSingle()
		{
			var tree = DuplicatesTree();

			Assert.Throws<InvalidOperationException>(
				() => tree.UpdateSingle(_scheme.Encrypt(3, _key), "", s => s.Contains('z'))
			);
		}

		[Fact]
		public void DuplicatePredicateDelete()
		{
			var output = new List<string>();

			var tree = DuplicatesTree();

			var result = tree.Delete(_scheme.Encrypt(6, _key), s => s.Contains('d'));
			Assert.True(result);

			Assert.False(tree.TryGet(_scheme.Encrypt(6, _key), null, s => s.Contains('d')));
		}
	}
}
