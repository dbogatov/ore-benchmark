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
		public void InsertNotUpdate()
		{
			var tree = ConstructTree(
				new Options<int, int>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3, 4, 5 }
			);

			Assert.True(tree.Insert(7, 7.ToString()));
		}

		[Fact]
		public void UpdateNotInsert()
		{
			var tree = ConstructTree(
				new Options<int, int>(
					new NoEncryptionScheme(),
					3
				),
				new List<int> { 3, 4, 5 }
			);

			Assert.False(tree.Insert(5, "five"));

			var result = "";
			tree.TryGet(5, out result);
			Assert.Equal("five", result);
		}

	}
}
