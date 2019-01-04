using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.CJJKRS;
using Simulation.Protocol.SSE;
using Xunit;

namespace Test.Simulators.Protocols.SSE
{
	public class TreeChecks
	{
		private class Keyword : IWord
		{
			public string Value { get; set; }

			public byte[] ToBytes() => Encoding.Default.GetBytes(Value);
		}

		private readonly Func<int, int, Keyword> encoder = (index, level) => new Keyword { Value = $"Index: {index}, level: {level}" };

		[Fact]
		public void NoIndices()
		{
			new Tree<Keyword>(-10, 10, new List<int>(), encoder);
		}

		[Fact]
		public void OneIndex()
		{
			new Tree<Keyword>(-10, 10, new List<int>() { 5 }, encoder);
		}

		[Fact]
		public void MultipleIndices()
		{
			new Tree<Keyword>(-10, 10, new List<int>() { 5, 6, 8 }, encoder);
		}


		[Fact]
		public void FirstInRangeFarLeft()
		{
			var tree = new Tree<Keyword>(-10, 10, new List<int>() { 5, 6, 8 }, encoder);
			Assert.Equal(5, tree.FirstInRange(3));
		}

		[Fact]
		public void FirstInRangeExact()
		{
			var tree = new Tree<Keyword>(-10, 10, new List<int>() { 5, 6, 8 }, encoder);
			Assert.Equal(6, tree.FirstInRange(6));
		}

		[Fact]
		public void FirstInRangeFarRight()
		{
			var tree = new Tree<Keyword>(-10, 10, new List<int>() { 5, 6, 8 }, encoder);
			Assert.Null(tree.FirstInRange(10));
		}

		[Fact]
		public void KeywordsForValue()
		{
			var tree = new Tree<Keyword>(-10, 10, new List<int>() { 5, 6, 8 }, encoder);
			var keywords = tree.KeywordsForValue(6);

			Assert.InRange(keywords.Count(), 5, 7);
			Assert.Contains("Index: 0, level: 0", keywords.Select(k => k.Value));
			Assert.Contains($"Index: 6, level: {keywords.Count() - 1}", keywords.Select(k => k.Value));
			for (int i = 0; i < keywords.Count(); i++)
			{
				Assert.True(keywords.Select(k => k.Value).Any(s => s.Contains($"level: {i}")));
			}
		}
	}
}
