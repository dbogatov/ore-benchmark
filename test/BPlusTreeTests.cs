using System;
using Xunit;
using DataStructures.BPlusTree;
using OPESchemes;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	public class BPlusTreeTests
	{
		[Fact]
		public void InitializeTest()
		{
			new Tree<int>(new Options());
		}

		[Fact]
		public void InsertSingleElementTest()
		{
			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			tree.Insert(3, "Three");
		}

		[Fact]
		public void TriggerRootSplitTest()
		{
			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			tree.Insert(3, "Three");
			tree.Insert(-2, "Minus two");
			tree.Insert(8, "Eight");
		}

		[Fact]
		public void TriggerInternalSplitTest()
		{
			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			tree.Insert(3, "Three");
			tree.Insert(-2, "Minus two");
			tree.Insert(8, "Eight");
			tree.Insert(6, "Six");
		}

		[Fact]
		public void FromLectureSlidesTest()
		{
			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			new List<int>() {
				30, 120, 100, 179, 5, 11, 200, 180, 150, 101, 3, 35, 110, 130, 156
			}.ForEach(val => tree.Insert(val, val.ToString()));

			// Console.WriteLine(tree.ToString());
		}

		[Fact]
		public void SquaresSeriesTest()
		{
			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			Enumerable
				.Range(1, 100)
				.Select(val => val * val)
				.ToList()
				.ForEach(val => tree.Insert(val, val.ToString()));

			// Console.WriteLine(tree.ToString());
		}

		[Fact]
		public void OscillatingSeriesTest()
		{
			const int max = 20;

			var tree = new Tree<string>(new Options(OPESchemes.OPESchemes.NoEncryption, 3));

			Enumerable
				.Range(1, max)
				.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * val + 2 * max)
				.ToList()
				.ForEach(val =>
				{
					Console.WriteLine($"Adding {val}");
					tree.Insert(val, val.ToString());
					Console.WriteLine(tree.ToString());
				});

		}


	}
}
