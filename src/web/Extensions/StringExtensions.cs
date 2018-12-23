using System;

namespace Web.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Parses string into enum of given type T.
		/// </summary>
		public static T ToEnum<T>(this string value) =>
			(T)Enum.Parse(typeof(T), value, true);
	}
}
