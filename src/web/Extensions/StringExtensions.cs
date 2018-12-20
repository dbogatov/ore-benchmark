using System;
using System.Globalization;

namespace Web.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Parses string into enum of given type T.
		/// </summary>
		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}

		/// <summary>
		/// Removes tabs and linebreaks from the string.
		/// </summary>
		/// <param name="input">String to be modified.</param>
		/// <returns>New string with all tabs and linebreaks removed.</returns>
		public static string RemoveWhitespaces(this string input)
		{
			return input
				.Replace(Environment.NewLine, string.Empty)
				.Replace("\t", string.Empty);
		}

		/// <summary>
		/// Truncates the string replacing all characters beyond maxChars with ...
		/// </summary>
		/// <param name="maxChars">The maximum length of the string after which it will be truncated</param>
		/// <returns>Truncated string</returns>
		public static string Truncate(this string value, int maxChars)
		{
			return new StringInfo(value).LengthInTextElements <= maxChars ? value : value.Substring(0, maxChars) + "...";
		}

		/// <summary>
		/// Contains method that honors StringComparison options.
		/// Needed for non-latin comparisons.
		/// </summary>
		/// <param name="source">String in which to find an occurrence</param>
		/// <param name="toCheck">Substring to find in source</param>
		/// <param name="comp">StringComparison options</param>
		/// <returns>True if toCheck is found in source and false otherwise</returns>
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source.IndexOf(toCheck, comp) >= 0;
		}
	}
}
