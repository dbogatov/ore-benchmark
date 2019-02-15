using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace BigNumbers
{
	class Program
	{
		static void Main(string[] args)
		{
			const int RUNS = 1000000;
			var G = new Random(123456);

			var ints = Enumerable
				.Range(1, RUNS)
				.Select(_ => (G.Next(), G.Next()));

			var bigs = Enumerable
				.Range(1, RUNS)
				.Select(_ =>
				{
					var first = new byte[192 / 8];
					var second = new byte[192 / 8];
					G.NextBytes(first);
					G.NextBytes(second);

					return (new BigInteger(first), new BigInteger(second));
				});

			Profile(ints, bigs, (a, b) => a + b, (a, b) => a + b, "addition");
			Profile(ints, bigs, (a, b) => a - b, (a, b) => a - b, "subtraction");
			Profile(ints, bigs, (a, b) => a * b, (a, b) => a * b, "multiplication");
			Profile(ints, bigs, (a, b) => a / b, (a, b) => a / b, "division");
			Profile(ints, bigs, (a, b) => a % b, (a, b) => a % b, "modulo");
			Profile(ints, bigs, (a, b) => { var _ = a > b; return 0; }, (a, b) => { var _ = a > b; return 0; }, "comparison");
		}

		static void Profile(
			IEnumerable<(int a, int b)> ints,
			IEnumerable<(BigInteger a, BigInteger b)> bigs,
			Func<int, int, int> intOperation,
			Func<BigInteger, BigInteger, BigInteger> bigOperation,
			string label
		)
		{
			TimeSpan printTime(Stopwatch sw, string numbers)
			{
				var ts = sw.Elapsed;

				string elapsedTime = String.Format(
					"{0:00}:{1:00}:{2:00}.{3:00}",
					ts.Hours,
					ts.Minutes,
					ts.Seconds,
					ts.Milliseconds / 10
				);
				Console.WriteLine($"{numbers} finished in " + elapsedTime);

				return ts;
			}

			Console.WriteLine($"Profiling {label} operation...");

			var stopWatchInts = new Stopwatch();
			var stopWatchBigs = new Stopwatch();

			stopWatchInts.Start();

			Regular(ints, intOperation);

			stopWatchInts.Stop();

			stopWatchBigs.Start();

			Big(bigs, bigOperation);

			stopWatchBigs.Stop();

			var intTs = printTime(stopWatchInts, "Regular");
			var bigTs = printTime(stopWatchBigs, "Big");

			Console.WriteLine($"Regular is {bigTs / intTs} times faster for {label} operation.");
			Console.WriteLine();
		}

		static void Regular(IEnumerable<(int a, int b)> input, Func<int, int, int> operation)
		{
			foreach (var pair in input)
			{
				operation(pair.a, pair.b);
			}
		}

		static void Big(IEnumerable<(BigInteger a, BigInteger b)> input, Func<BigInteger, BigInteger, BigInteger> operation)
		{
			foreach (var pair in input)
			{
				operation(pair.a, pair.b);
			}
		}
	}
}
