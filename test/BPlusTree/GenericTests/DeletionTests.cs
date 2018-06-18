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
		public Tree<string, C> DeleteAndValidate(Tree<string, C> tree, int element, bool print = false)
		{
			if (print)
			{
				Console.WriteLine($"Deleting {element}");
			}

			Assert.True(tree.Delete(_scheme.Encrypt(element, _key)));

			if (print)
			{
				Console.WriteLine(tree.ToString());
			}
			Assert.False(tree.TryGet(_scheme.Encrypt(element, _key), out _));

			Assert.True(tree.Validate());

			return tree;
		}

		[Fact]
		public void DeleteNotExistingElementTest()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, 5 }
			);

			Assert.False(tree.Delete(_scheme.Encrypt(4, _key)));
		}

		[Fact]
		public void DeleteEmptyTreeTest()
		{
			var tree = new Tree<string, C>(
				_defaultOptions
			);

			Assert.False(tree.Delete(_scheme.Encrypt(2, _key)));
		}

		[Fact]
		public void DeleteSingleElementTest()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			);

			DeleteAndValidate(tree, 3);
		}

		[Fact]
		public void DeleteMultilevelTest()
		{
			var tree = ConstructTree(
				_defaultOptions,
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
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			);

			DeleteAndValidate(tree, 58 * 58);
			DeleteAndValidate(tree, 59 * 59);
			tree.Insert(_scheme.Encrypt(59 * 59, _key), (59 * 59).ToString());
			Assert.True(tree.TryGet(_scheme.Encrypt(59 * 59, _key), out _));
			DeleteAndValidate(tree, 59 * 59);
		}

		[Fact]
		public void DeleteLargestElement()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8, 6, 20, 21, 11 }
			);

			DeleteAndValidate(tree, 21);
		}

		[Fact]
		public void DeleteWithMergeToRight()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8, 6, 20, 21, 11, 12, 22 }
			);

			DeleteAndValidate(tree, 6);
			DeleteAndValidate(tree, 8);
		}

		[Fact]
		public void DeleteWithBorrowFromLeft()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8, 6, 20, 21, 11, 12, -5, -10 }
			);

			DeleteAndValidate(tree, 11);
			DeleteAndValidate(tree, 12);
			DeleteAndValidate(tree, 20);
		}

		[Fact]
		public void DeleteWithBorrowFromRight()
		{
			var tree = ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8, 6, 20, 21, 22, 23, 11, 12 }
			);

			DeleteAndValidate(tree, 6);
			DeleteAndValidate(tree, 8);
		}

		[Fact]
		public void DeleteRandomElementsTest()
		{
			Random random = new Random(3068354); // seed is static

			int max = _max / 10;

			for (int i = 3; i < 11; i++)
			{
				var input =
					Enumerable
						.Range(1, max)
						.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * random.Next(max) + 2 * max)
						.Distinct()
						.ToList();
				var tree = ConstructTree(
					OptionsWithBranching(i),
					input
				);

				for (int j = 0; j < input.Count; j++)
				{
					if (j % 10 == 0)
					{
						// delete non existing element
						Assert.False(tree.Delete(_scheme.Encrypt(5 * max + random.Next(max), _key)));
					}

					// delete an element from the tree and validate the structure
					var value = input[random.Next(input.Count)];
					input.Remove(value);
					DeleteAndValidate(tree, value);
				}
			}
		}
	}
}
