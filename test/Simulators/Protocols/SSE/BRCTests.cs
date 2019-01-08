using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Simulation.Protocol.SSE;
using Xunit;

namespace Test.Simulators.Protocols.SSE
{
	public class BRC
	{
		private readonly bool _print = true;

		private string BitArrayToString(BitArray bits)
			=> string.Join("", bits.Cast<bool>().Reverse().Select(b => (b ? 1 : 0).ToString()));

		private void PrintBRCResult(List<(BitArray, int)> result)
		{
			if (_print)
			{
				foreach (var node in result)
				{
					Console.WriteLine($"{{ {node.Item2.ToString().PadRight(2)}, {BitArrayToString(node.Item1)} }}");
				}
			}
		}

		private void PrintInputs(uint a, uint b, int n = -1)
		{
			n = n < 0 ? sizeof(uint) * 8 : n;
			string UIntBits(uint x)
				=> string.Join("", new BitArray(BitConverter.GetBytes(x)).Cast<bool>().Take(n).Reverse().Select(bit => (bit ? 1 : 0).ToString()));

			Console.WriteLine($"From ({a.ToString().PadLeft(3)}): {UIntBits(a)}");
			Console.WriteLine($"To   ({b.ToString().PadLeft(3)}): {UIntBits(b)}");
		}

		private void AssertSolution(string[] expected, List<(BitArray, int)> actual, int n)
		{
			Assert.Equal(expected.Length, actual.Count);

			Assert.All(actual, t => Assert.Equal(t.Item1.Length + t.Item2, n));

			foreach (var correct in expected)
			{
				Assert.True(actual.Any(t => BitArrayToString(t.Item1).Equals(correct)));
			}
		}

		[Theory]
		[InlineData(2, 7, 4, new string[] { "001", "01" })] // Paper example
		[InlineData(1, 7, 4, new string[] { "0001", "001", "01" })]
		[InlineData(4, 5, 4, new string[] { "010" })]
		[InlineData(3, 12, 4, new string[] { "01", "0011", "10", "1100" })]
		[InlineData(11, 12, 4, new string[] { "1011", "1100" })]
		[InlineData(0, 15, 4, new string[] { "" })]
		[InlineData(0, 7, 4, new string[] { "0" })]
		[InlineData(1, 14, 4, new string[] { "0001", "001", "01", "10", "110", "1110" })]
		public void PrecomputedCheck(uint a, uint b, int n, string[] correct)
		{
			var result = Cover.BRC(a, b, n);

			PrintInputs(a, b, n);
			PrintBRCResult(result);

			AssertSolution(correct, result, n);
		}
	}
}
