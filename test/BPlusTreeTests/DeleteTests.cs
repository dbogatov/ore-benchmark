using System;
using Xunit;
using DataStructures.BPlusTree;
using OPESchemes;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	public partial class BPlusTreeTests
	{
		public Tree<string> DeleteAndValidate(Tree<string> tree, int element)
		{
			Assert.True(tree.Delete(element));
			Assert.False(tree.TryGet(element, out _));
			tree.Validate();

			return tree;
		}

		[Fact]
		public void DeleteNotExistingElementTest()
		{
			var tree = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3 }
			);

			DeleteAndValidate(tree, 3);
		}

		[Fact]
		public void DeleteEmptyTreeTest()
		{
			var tree = new Tree<string>(new Options());

			DeleteAndValidate(tree, 2);
		}

		[Fact]
		public void DeleteSingleElementTest()
		{
			var tree = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3 }
			);

			DeleteAndValidate(tree, 3);
		}

		[Fact]
		public void DeleteMultilevelTest()
		{
			var tree = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			);

			DeleteAndValidate(tree, 58 * 58);
		}

		[Fact]
		public void DeleteThenInsertTest()
		{
			var tree = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			);

			DeleteAndValidate(tree, 58 * 58);
			DeleteAndValidate(tree, 59 * 59);
			tree.Insert(59 * 59, (59 * 59).ToString());
			Assert.True(tree.TryGet(59 * 59, out _));
			DeleteAndValidate(tree, 59 * 59);
		}

		[Fact]
		public void DeleteWithBorrow()
		{
			var tree = ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3, -2, 8, 6, 20, 21, 22, 23, 11, 12 },
				true, true
			);

			DeleteAndValidate(tree, 6);
			DeleteAndValidate(tree, 8);
			Console.WriteLine(tree.ToString());
		}

		[Fact]
		public void DeleteRandomElementsTest()
		{
			const int max = 1000;
			Random random = new Random(3068354); // seed is static

			for (int i = 3; i < 11; i++)
			{
				var input =
					Enumerable
						.Range(1, max)
						.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * random.Next(max) + 2 * max)
						.ToList();
				var tree = ConstructTree(
					new Options(OPESchemes.OPESchemes.NoEncryption, i),
					input
				);

				for (int j = 0; j < max / 5; j++)
				{
					if (j % 10 == 0)
					{
						// delete non existing element
						Assert.False(tree.Delete(3 * max + random.Next(max)));
					}
					else
					{
						// delete an element from the tree and validate the structure
						var value = input[random.Next(input.Count)];
						input.Remove(value);
						DeleteAndValidate(tree, value);
					}
				}
			}
		}
	}
}
