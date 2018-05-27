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
	}
}
