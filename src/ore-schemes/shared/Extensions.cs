using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.PRP;

namespace ORESchemes.Shared
{
	public static class Extensions
	{
		// https://stackoverflow.com/a/38519324/1644554
		/// <summary>
		/// Converts byte array to a readable string
		/// </summary>
		public static string Print(this byte[] byteArray)
		{
			var sb = new StringBuilder("{ ");
			for (var i = 0; i < byteArray.Length; i++)
			{
				var b = byteArray[i];
				sb.Append(b.ToString().PadLeft(3));
				if (i < byteArray.Length - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append(" }");
			return sb.ToString();
		}

		// https://stackoverflow.com/a/623184/1644554
		/// <summary>
		/// Converts byte array to a hex string all upper-case no spaces
		/// </summary>
		public static string PrintHex(this byte[] byteArray) =>
			BitConverter.ToString(byteArray).Replace("-", string.Empty).ToUpper();

		// https://stackoverflow.com/a/2253903/1644554
		/// <summary>
		/// Returns standard deviation of series of integers
		/// </summary>
		public static double StdDev(this IEnumerable<int> values)
		{
			double ret = 0;
			int count = values.Count();
			if (count > 1)
			{
				// Compute the Average
				double avg = values.Average();

				// Perform the Sum of (value-avg)^2
				double sum = values.Sum(d => (d - avg) * (d - avg));

				// Put it all together
				ret = Math.Sqrt(sum / count);
			}
			return ret;
		}

		/// <summary>
		/// Transforms signed int64 to unsigned int64 by shifting the value by int64 min value
		/// </summary>
		public static ulong ToULong(this long value) => unchecked((ulong)(value + Int64.MinValue));

		/// <summary>
		/// Transforms unsigned int64 to signed int64 by shifting the value by int64 min value
		/// </summary>
		public static long ToLong(this ulong value) => (long)(value - unchecked((ulong)Int64.MinValue));

		/// <summary>
		/// Transforms unsigned int32 to signed int32 by shifting the value by int32 min value
		/// </summary>
		public static int ToInt(this uint value) => (int)(value - Int32.MinValue);

		/// <summary>
		/// Transforms signed int32 to unsigned int32 by shifting the value by int32 min value
		/// </summary>
		public static uint ToUInt(this int value) => unchecked((uint)(value + Int32.MinValue));

		/// <summary>
		/// Returns the largest possible ciphertext value possible as an encryption of largest
		/// possible plaintext, which is assumed to be max int32.
		/// </summary>
		public static C MaxCiphertextValue<C, K>(this IOREScheme<C, K> scheme, K key)
			where C : IGetSize
			where K : IGetSize
		=> scheme.Encrypt(int.MaxValue, key);

		/// <summary>
		/// Returns the smallest possible ciphertext value possible as an encryption of smallest
		/// possible plaintext, which is assumed to be min int32.
		/// </summary>
		public static C MinCiphertextValue<C, K>(this IOREScheme<C, K> scheme, K key)
			where C : IGetSize
			where K : IGetSize
		=> scheme.Encrypt(int.MinValue, key);

		/// <summary>
		/// Helper that returns an array of uniformly sampled bytes of given size
		/// </summary>
		public static byte[] GetBytes(this IPRG G, int n)
		{
			byte[] bytes = new byte[n];
			G.NextBytes(bytes);
			return bytes;
		}

		/// <summary>
		/// Helper that returns an array of uniformly sampled bytes of given size
		/// </summary>
		public static byte[] GetBytes(this Random G, int n)
		{
			byte[] bytes = new byte[n];
			G.NextBytes(bytes);
			return bytes;
		}

		/// <summary>
		/// Helper that wraps PRP when uint input is provided instead of generic bit array
		/// </summary>
		public static uint Permute(this IPRP P, uint input, byte[] key, int bits)
		{
			BitArray permutation =
				P.PRP(
					new BitArray(new int[] { (int)input }),
					key,
					bits
				);
			int[] result = new int[1];
			permutation.CopyTo(result, 0);

			return (uint)result[0];
		}

		/// <summary>
		/// Helper that wraps PRP when uint input is provided instead of generic bit array
		/// </summary>
		public static uint Unpermute(this IPRP P, uint input, byte[] key, int bits)
		{
			BitArray permutation =
				P.InversePRP(
					new BitArray(new int[] { (int)input }),
					key,
					bits
				);
			int[] result = new int[1];
			permutation.CopyTo(result, 0);

			return (uint)result[0];
		}

		// https://stackoverflow.com/a/1262619/1644554
		/// <summary>
		/// Helper routine that shuffles a list using Knuth shuffle using entropy generated
		/// by the provided Random generator
		/// </summary>
		public static IList<T> Shuffle<T>(this IList<T> list, Random rng)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}

			return list;
		}

		// https://stackoverflow.com/a/3773438/1644554
		public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
		{
			List<T> toReturn = new List<T>(max);
			foreach (var item in source)
			{
				toReturn.Add(item);
				if (toReturn.Count == max)
				{
					yield return toReturn;
					toReturn = new List<T>(max);
				}
			}
			if (toReturn.Any())
			{
				yield return toReturn;
			}
		}
	}
}
