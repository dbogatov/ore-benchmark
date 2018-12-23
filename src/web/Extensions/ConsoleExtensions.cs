using System;

namespace Web.Extensions
{
	public static class ColoredConsole
	{
		/// <summary>
		/// Writes colored text to the output. Resets color when done.
		/// </summary>
		/// <param name="message">Text to be put to output.</param>
		/// <param name="foreground">Foreground color.</param>
		/// <param name="background">Background color.</param>
		public static void Write(
			string message,
			ConsoleColor? foreground = null,
			ConsoleColor? background = null)
		{
			if (background.HasValue)
			{
				Console.BackgroundColor = background.Value;
			}

			if (foreground.HasValue)
			{
				Console.ForegroundColor = foreground.Value;
			}

			Console.Write(message);

			Console.ResetColor();
		}

		/// <summary>
		/// Writes colored text to the output. Appends new line. Resets color when done.
		/// </summary>
		/// <param name="message">Text to be put to output.</param>
		/// <param name="foreground">Foreground color.</param>
		/// <param name="background">Background color.</param>
		public static void WriteLine(
			string message,
			ConsoleColor? foreground = null,
			ConsoleColor? background = null)
		{
			ColoredConsole.Write(message + Environment.NewLine, foreground, background);
		}
	}
}
