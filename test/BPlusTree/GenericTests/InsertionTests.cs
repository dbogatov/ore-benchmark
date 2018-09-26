using System;
using Xunit;
using BPlusTree;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
	{
		protected virtual ITree<string, C> GetTree(Options<C> options) => new Tree<string, C>(options);
		
		private ITree<string, C> ConstructTree(Options<C> options, List<int> input, bool print = false, bool validate = true, List<string> data = null)
		{
			var tree = new Tree<string, C>(options);

			for (int i = 0; i < input.Count; i++)
			{
				var val = input[i];
				var d = data == null ? val.ToString() : data[i];

				if (print)
				{
					Console.WriteLine($"Adding {val}");
				}
				tree.Insert(_scheme.Encrypt(val, _key), d);
				if (validate)
				{
					Assert.True(tree.Validate());
				}
				if (print)
				{
					Console.WriteLine(tree.ToString());
				}
			}

			return tree;
		}

		[Fact]
		public void Initialize()
			=> ConstructTree(_defaultOptions, new List<int>());

		[Fact]
		public void InsertSingleElement()
			=> ConstructTree(
				_defaultOptions,
				new List<int> { 3 }
			);

		[Fact]
		public void TriggerRootSplit()
			=> ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8 }
			);

		[Fact]
		public void TriggerInternalSplit()
			=> ConstructTree(
				_defaultOptions,
				new List<int> { 3, -2, 8, 6 }
			);

		[Fact]
		public void FromLectureSlides()
			=> ConstructTree(
				_defaultOptions,
				new List<int> { 30, 120, 100, 179, 5, 11, 200, 180, 150, 101, 3, 35, 110, 130, 156 }
			);

		[Fact]
		public void SquaresSeries()
			=> ConstructTree(
				_defaultOptions,
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			);

		[Fact]
		public void OscillatingSeries()
			=> ConstructTree(
				OptionsWithBranching(5),
				Enumerable
					.Range(1, _max)
					.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * val + 2 * _max)
					.ToList()
			);

		[Fact]
		public void RandomSequence()
		{
			Random random = new Random(3068354); // seed is static

			int max = _max / 10;

			for (int i = 3; i < 11; i++)
			{
				ConstructTree(
					OptionsWithBranching(i),
					Enumerable
						.Range(1, max)
						.Select(val => (val % 2 == 0 ? -1 : 1) * 2 * random.Next(max) + 2 * max)
						.ToList()
				);
			}
		}
	}
}
