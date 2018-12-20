using System;
using System.Collections.Generic;
using System.Linq;
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
