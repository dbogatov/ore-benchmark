using System;
using Xunit;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C>
	{
		[Fact]
		public void SearchRangeNotExistingTest()
		{
			var result = ConstructTree(
				new Options<C>(
					_scheme,
					3
				),
				new List<int> { 3 }
			).TryRange(_scheme.Encrypt(5, _key), _scheme.Encrypt(10, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeEmptyTreeTest()
		{
			var result = new Tree<string, C>(
				new Options<C>(
					_scheme,
					3
				)
			).TryRange(_scheme.Encrypt(2, _key), _scheme.Encrypt(3, _key), out _);

			Assert.False(result);
		}

		[Fact]
		public void SearchRangeSingleElementTest()
		{
			List<string> output = null;

			var result = ConstructTree(
				new Options<C>(
					_scheme,
					3
				),
				new List<int> { 3 }
			).TryRange(_scheme.Encrypt(2, _key), _scheme.Encrypt(4, _key), out output);

			Assert.True(result);

			Assert.Equal(new List<string> { 3.ToString() }, output);
		}

		[Theory]
		[InlineData(58, 6545)]
		[InlineData(5000, 5050)]
		[InlineData(3000, 7000)]
		[InlineData(1, 10000)]
		public void SearchRangeMultilevelTest(int from, int to)
		{
			List<string> output = null;
			const int salt = 123;
			const int originalMax = 10000;

			from = (int)Math.Round(((double)from / originalMax) * _max);
			to = (int)Math.Round(((double)to / originalMax) * _max);

			if (from == to)
			{
				to++;
			}

			if (from == 0)
			{
				from++;
			}

			var result = ConstructTree(
				new Options<C>(
					_scheme,
					3
				),
				Enumerable
					.Range(1, _max)
					.Select(val => val * salt)
					.ToList(),
				false,
				false
			).TryRange(_scheme.Encrypt(from * salt, _key), _scheme.Encrypt(to * salt, _key), out output);

			Assert.True(result);

			Assert.Equal(
				Enumerable
					.Range(from, to - from + 1)
					.Select(val => (val * salt).ToString())
					.OrderBy(val => val)
					.ToList(),
				output
					.OrderBy(val => val)
					.ToList()
			);
		}

		[Fact]
		public void SearchRangeNonExistingMultilevelTest()
		{
			var result = ConstructTree(
				new Options<C>(
					_scheme,
					3
				),
				Enumerable
					.Range(1, 100)
					.Select(val => val * val)
					.ToList()
			).TryRange(_scheme.Encrypt(101 * 101, _key), _scheme.Encrypt(120 * 120, _key), out _);

			Assert.False(result);
		}

		[Theory]
		[InlineData(3, 2)]
		[InlineData(2, 2)]
		public void SearchRangeEndBeforeStartTest(int start, int end)
		{
			Assert.Throws<ArgumentException>(
				() =>
				new Tree<string, C>(
					new Options<C>(
						_scheme,
						3
					)
				).TryRange(_scheme.Encrypt(start, _key), _scheme.Encrypt(end, _key), out _)
			);
		}
	}
}
