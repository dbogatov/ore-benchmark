using System;
using Xunit;
using BPlusTree;
using System.Collections.Generic;
using System.Linq;
using Crypto.Shared;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
	{

		[Fact]
		public void PrintTree()
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
		public void MalformedOptions(int branches)
			=> Assert.Throws<ArgumentException>(
				() => new Options<C>(_scheme, branches)
			);

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(10)]
		public void Size(int expected)
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

		[Fact]
		public void CustomVisitHandler()
		{
			// Arrange
			HashSet<int> hashes = new HashSet<int>();

			var options = new Options<C>(_scheme, 3);
			options.MinCipher = _scheme.MinCiphertextValue(_key);
			options.MaxCipher = _scheme.MaxCiphertextValue(_key);
			options.NodeAccessHandler = hash => hashes.Add(hash);

			// Act
			var tree = ConstructTree(
				options,
				Enumerable.Range(1, 5).ToList()
			);

			// Assert
			Assert.True(hashes.Count >= 5);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(10)]
		[InlineData(50)]
		public void Nodes(int expected)
		{
			var options = new Options<C>(_scheme, 3);
			options.MinCipher = _scheme.MinCiphertextValue(_key);
			options.MaxCipher = _scheme.MaxCiphertextValue(_key);

			var tree = ConstructTree(
				options,
				expected > 0 ?
					Enumerable
						.Range(1, expected * 3)
						.ToList()
					:
					new List<int>()
			);

			var withDataNodes = tree.Nodes();
			var withoutDataNodes = tree.Nodes(includeDataNodes: false);

			Assert.True(withDataNodes >= expected);
			Assert.True(withoutDataNodes >= expected);
			Assert.True(expected > 0 ? withDataNodes > withoutDataNodes : withDataNodes == withoutDataNodes);
		}
	}
}
