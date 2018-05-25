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
		private int ProfileVisitedNodes(Action<Tree<string, long, int>> routine, List<int> seeds = null)
		{
			var options = new Options<int, long>(
				new NoEncryptionScheme(),
				3
			);
			var tree = new Tree<string, long, int>(options);

			if (seeds != null)
			{
				seeds.ForEach(val => tree.Insert(val, val.ToString()));
			}

			var visited = new HashSet<int>();
			options.NodeVisited += new NodeVisitedEventHandler((hash) => visited.Add(hash));

			routine(tree);

			return visited.Count();
		}


		[Fact]
		public void InsertionEventsTest()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 100)
						.ToList()
						.ForEach(val => tree.Insert(val, val.ToString()))
			);

			Assert.InRange(visited, 100, 300);
		}

		[Fact]
		public void UpdateEventsTest()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.Insert(val, (val + 100).ToString())),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void DeleteEventsTest()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.Delete(val)),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void SearchEventsTest()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.TryGet(val, out _)),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void SearchRangeEventsTest()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => new
						{
							from = val * 10,
							to = val * 15
						})
						.ToList()
						.ForEach(val => tree.TryRange(val.from, val.to, out _)),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 100, 300);
		}
	}
}
