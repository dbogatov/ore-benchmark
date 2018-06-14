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
		public void InsertNotUpdate()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 4, 5 }
			);

			Assert.True(tree.Insert(_scheme.Encrypt(7, _key), 7.ToString()));
		}

		[Fact]
		public void UpdateNotInsert()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 4, 5 }
			);

			Assert.False(tree.Insert(_scheme.Encrypt(5, _key), "five"));

			var result = "";
			tree.TryGet(_scheme.Encrypt(5, _key), out result);
			Assert.Equal("five", result);
		}

	}
}
