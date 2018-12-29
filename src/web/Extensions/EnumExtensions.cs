using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Web.Extensions
{
	public static class EnumExtensions
	{
		/// <summary>
		/// Returns the int value of the enum
		/// </summary>
		/// <returns>The int value of the enum</returns>
		public static int AsInt(this Enum value)
		{
			return Convert.ToInt32(value);
		}

		/// <summary>
		/// Returns a LogLevelLabel object for a given LogLevel (severity)
		/// </summary>
		/// <param name="level">Log message severity</param>
		/// <returns>LogLevelLabel object which defines how to print a label for a given severity.</returns>
		public static LogLevelLabel GetLabel(this LogLevel level)
		{
			var tuples = new Dictionary<LogLevel, LogLevelLabel>() {
				{
					LogLevel.Information,
					new LogLevelLabel {
						Text = "INFO",
						ForegroundColor = ConsoleColor.Green
					}
				},
				{
					LogLevel.Error,
					new LogLevelLabel {
						Text = "ERROR",
						ForegroundColor = ConsoleColor.Red
					}
				},
				{
					LogLevel.Critical,
					new LogLevelLabel {
						Text = "FATAL",
						BackgroundColor = ConsoleColor.DarkRed,
						ForegroundColor = ConsoleColor.White
					}
				},
				{
					LogLevel.Debug,
					new LogLevelLabel {
						Text = "DEBUG",
						ForegroundColor = ConsoleColor.White
					}
				},
				{
					LogLevel.Trace,
					new LogLevelLabel {
						Text = "TRACE",
						ForegroundColor = ConsoleColor.Gray
					}
				},
				{
					LogLevel.Warning,
					new LogLevelLabel {
						Text = "WARN",
						ForegroundColor = ConsoleColor.Yellow
					}
				}
			};

			// Make all label equal width - the width of the longest label
			var wordLength = tuples.Values.Max((label) => label.Text.Length);
			foreach (var tuple in tuples)
			{
				tuples[tuple.Key].Text = tuples[tuple.Key].Text.PadLeft(wordLength);
			}

			return tuples[level];
		}

		/// <summary>
		/// Return the value of Display property.
		/// https://stackoverflow.com/a/479417/1644554
		/// </summary>
		/// <param name="enumerationValue">The enum argument for which to extract value</param>
		/// <typeparam name="T">The specific enum type</typeparam>
		/// <returns>Value of Display attribute, or this enum ToString if attribute no supplied</returns>
		public static string GetDescription<T>(this T enumerationValue) where T : Enum
		{
			Type type = enumerationValue.GetType();

			MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

				if (attrs != null && attrs.Length > 0)
				{
					return ((DisplayAttribute)attrs[0]).Name;
				}
			}

			return enumerationValue.ToString();
		}
	}

	/// <summary>
	/// Helper class defining parameters on how to print a label for log message
	/// </summary>
	public class LogLevelLabel
	{
		/// <summary>
		/// Human readable representation of severity
		/// </summary>
		public string Text { get; set; }
		public ConsoleColor? BackgroundColor { get; set; }
		public ConsoleColor? ForegroundColor { get; set; }
	}
}
