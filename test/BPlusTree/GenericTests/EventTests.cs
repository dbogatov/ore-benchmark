using System;
using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
	{
		private int ProfileVisitedNodes(Action<Tree<string, C>> routine, List<int> seeds = null)
		{
			var options = _defaultOptions;
			var tree = new Tree<string, C>(options);

			if (seeds != null)
			{
				seeds.ForEach(val => tree.Insert(_scheme.Encrypt(val, _key), val.ToString()));
			}

			var visited = new HashSet<int>();
			options.NodeVisited += new NodeVisitedEventHandler((hash) => visited.Add(hash));

			routine(tree);

			return visited.Count();
		}


		[Fact]
		public void InsertionEvents()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 100)
						.ToList()
						.ForEach(val => tree.Insert(_scheme.Encrypt(val, _key), val.ToString()))
			);

			Assert.InRange(visited, 100, 300);
		}

		[Fact]
		public void UpdateEvents()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.Insert(_scheme.Encrypt(val, _key), (val + 100).ToString())),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void DeleteEvents()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.Delete(_scheme.Encrypt(val, _key))),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void SearchEvents()
		{
			var visited = ProfileVisitedNodes(
				tree =>
					Enumerable
						.Range(1, 5)
						.Select(val => val * 15)
						.ToList()
						.ForEach(val => tree.TryGet(_scheme.Encrypt(val, _key), null)),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 25, 100);
		}

		[Fact]
		public void SearchRangeEvents()
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
						.ForEach(val => tree.TryRange(_scheme.Encrypt(val.from, _key), _scheme.Encrypt(val.to, _key), null)),
				Enumerable
					.Range(1, 100)
					.ToList()
			);

			Assert.InRange(visited, 100, 300);
		}
	}
}
