// using System;
// using Xunit;
// using DataStructures.BPlusTree;
// using OPESchemes;
// using System.Collections.Generic;
// using System.Linq;

// namespace Test
// {
// 	public partial class BPlusTreeTests
// 	{
// 		[Fact]
// 		public void DeleteNotExistingElementTest()
// 		{
// 			var result = ConstructTree(
// 				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
// 				new List<int> { 3 }
// 			).Delete(2);

// 			Assert.False(result);
// 		}

// 		[Fact]
// 		public void DeleteEmptyTreeTest()
// 		{
// 			var result = new Tree<string>(new Options()).Delete(2);

// 			Assert.False(result);
// 		}

// 		[Fact]
// 		public void DeleteSingleElementTest()
// 		{
// 			var tree = ConstructTree(
// 				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
// 				new List<int> { 3 }
// 			);

// 			Assert.True(tree.Delete(3));
// 			Assert.False(tree.TryGet(3, out _));
// 		}

// 		[Fact]
// 		public void DeleteMultilevelTest()
// 		{
// 			var tree = ConstructTree(
// 				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
// 				Enumerable
// 					.Range(1, 100)
// 					.Select(val => val * val)
// 					.ToList()
// 			);

// 			Assert.True(tree.Delete(58 * 58));
// 			Assert.False(tree.TryGet(58 * 58, out _));
// 		}

// 		[Fact]
// 		public void DeleteThenInsertTest()
// 		{
// 			var tree = ConstructTree(
// 				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
// 				Enumerable
// 					.Range(1, 100)
// 					.Select(val => val * val)
// 					.ToList()
// 			);

// 			Assert.True(tree.Delete(58 * 58));
// 			Assert.True(tree.Delete(59 * 59));
// 			Assert.False(tree.TryGet(59 * 59, out _));
// 			tree.Insert(59 * 59, (59 * 59).ToString());
// 			Assert.True(tree.TryGet(59 * 59, out _));
// 			Assert.True(tree.Delete(59 * 59));
// 			Assert.False(tree.TryGet(59 * 59, out _));
// 		}
// 	}
// }
