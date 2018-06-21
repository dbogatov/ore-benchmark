using System;
using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C, K>
	{

		[Fact]
		public void PrintTreeTest()
		{
			var input = new List<int> { 3, -2, 8 };
			var tree = ConstructTree(
				_defaultOptions,
				input,
				false, false
			);

			var description = tree.ToString();

			input.ForEach(
				i => Assert.Contains($"\"{i}\"", description)
			);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(70000)]
		public void MalformedOptionsTest(int branches)
		{
			Assert.Throws<ArgumentException>(
				() => new Options<C>(_scheme, branches)
			);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(10)]
		public void SizeTest(int expected)
		{
			var tree = ConstructTree(
				_defaultOptions,
				expected > 0 ?
					Enumerable
						.Range(1, expected)
						.ToList()
					:
					new List<int>()
			);

			Assert.Equal(expected, tree.Size());
		}
	}
}
