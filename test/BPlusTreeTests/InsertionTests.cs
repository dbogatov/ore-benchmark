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
		private Tree<string> ConstructTree(Options options, List<int> input, bool print = false)
		{
			var tree = new Tree<string>(options);

			input
				.ForEach(val =>
				{
					if (print)
					{
						Console.WriteLine($"Adding {val}");
					}
					tree.Insert(val, val.ToString());
					Assert.True(tree.Validate());
					if (print)
					{
						Console.WriteLine(tree.ToString());
					}
				});

			return tree;
		}

		[Fact]
		public void InitializeTest()
		{
			new Tree<int>(new Options());
		}

		[Fact]
		public void InsertSingleElementTest()
		{
			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3 }
			);
		}

		[Fact]
		public void TriggerRootSplitTest()
		{
			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3, -2, 8 }
			);
		}

		[Fact]
		public void TriggerInternalSplitTest()
		{
			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 3, -2, 8, 6 }
			);
		}

		[Fact]
		public void FromLectureSlidesTest()
		{
			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				new List<int> { 30, 120, 100, 179, 5, 11, 200, 180, 150, 101, 3, 35, 110, 130, 156 }
			);
		}

		[Fact]
		public void SquaresSeriesTest()
		{
			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 3),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			);
		}

		[Fact]
		public void OscillatingSeriesTest()
		{
			const int max = 1000;

			ConstructTree(
				new Options(OPESchemes.OPESchemes.NoEncryption, 5),
				Enumerable
					.Range(1, max)
					.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * val + 2 * max)
					.ToList()
			);
		}

		[Fact]
		public void RandomSequenceTest()
		{
			const int max = 1000;
			Random random = new Random(3068354); // seed is static

			for (int i = 3; i < 11; i++)
			{
				ConstructTree(
					new Options(OPESchemes.OPESchemes.NoEncryption, i),
					Enumerable
						.Range(1, max)
						.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * random.Next(max) + 2 * max)
						.ToList()
				);
			}
		}
	}
}
