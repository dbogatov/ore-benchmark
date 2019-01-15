using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.Shared.Primitives.PRG;

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
		/// Returns a given number of pseudo-randomly sampled bits
		/// </summary>
		public static BitArray GetBits(this IPRG G, int n)
			=> new BitArray(new BitArray(G.GetBytes((n + 7) / 8)).Cast<bool>().Take(n).ToArray());

		/// <summary>
		/// Helper that returns an array of uniformly sampled bytes of given size
		/// </summary>
		public static byte[] GetBytes(this Random G, int n)
		{
			byte[] bytes = new byte[n];
			G.NextBytes(bytes);
			return bytes;
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
		/// <summary>
		/// Partitions a list to lists of max elements and returns that list
		/// </summary>
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

		// https://stackoverflow.com/a/518558/1644554
		/// <summary>
		/// Prepends a BitArray to this one, returns a new BitArray instance
		/// </summary>
		public static BitArray Prepend(this BitArray current, BitArray before)
		{
			var bools = new bool[current.Count + before.Count];
			before.CopyTo(bools, 0);
			current.CopyTo(bools, before.Count);
			return new BitArray(bools);
		}

		/// <summary>
		/// Returns true if two BitArray instances have equal content
		/// </summary>
		public static bool IsEqualTo(this BitArray current, BitArray other)
		{
			if (current.Length != other.Length)
			{
				return false;
			}

			return new BitArray(current).Xor(new BitArray(other)).Not().Cast<bool>().All(e => e);
		}

		// https://stackoverflow.com/a/4619295/1644554
		/// <summary>
		/// Converts BitArray instance to an array of bytes
		/// </summary>
		public static byte[] ToBytes(this BitArray bits)
		{
			if (bits.Length == 0)
			{
				return new byte[] { };
			}

			byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
			bits.CopyTo(ret, 0);
			return ret;
		}

		/// <summary>
		/// An override of BitArray default GetHashCode.
		/// This one takes into account the contents of the array, not its address.
		/// </summary>
		public static int ProperHashCode(this BitArray bits)
			=> bits.ToBytes().ProperHashCode();

		/// <summary>
		/// An override of byte array default GetHashCode.
		/// This one takes into account the contents of the array, not its address.
		/// </summary>
		public static int ProperHashCode(this byte[] bytes)
		{
			unchecked
			{
				if (bytes == null)
				{
					return 0;
				}
				int hash = 17;
				foreach (var @byte in bytes)
				{
					hash = hash * 31 + @byte;
				}
				return hash;
			}
		}

		/// <summary>
		/// A convenient extension for applying Distinct combinator for property of an object.
		/// https://stackoverflow.com/a/489421/1644554
		/// </summary>
		/// <param name="keySelector">Lambda that returns a key for which Distinct needs to compute</param>
		/// <typeparam name="TSource">Type of the enumerable element</typeparam>
		/// <typeparam name="TKey">Type of the key</typeparam>
		/// <returns>
		/// This list with all elements filtered such that particular property
		/// identified by the key is distinct
		/// </returns>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
	}
}
