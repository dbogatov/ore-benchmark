using Xunit;
using System.Collections.Generic;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
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

			Assert.True(tree.UpdateSingle(_scheme.Encrypt(5, _key), "five"));

			var result = "";
			tree.TryGetSingle(_scheme.Encrypt(5, _key), out result);
			Assert.Equal("five", result);
		}
	}
}
