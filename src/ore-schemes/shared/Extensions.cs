using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public static C MaxCiphertextValue<C, K>(this IOREScheme<C, K> scheme, K key) => scheme.Encrypt(int.MaxValue, key);
		public static C MinCiphertextValue<C, K>(this IOREScheme<C, K> scheme, K key) => scheme.Encrypt(int.MinValue, key);
	}
}
