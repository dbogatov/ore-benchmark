using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Web.Extensions
{
	public static class ConfigurationExtensions
	{
		/// <summary>
		/// This helper methods returns an enumerable (list) of sections for the section of config.
		/// For example, if config contains a JSON array like this:
		/// 
		/// Data : [
		/// 	{
		/// 		FirstName: Dmytro
		/// 		LastName: Bogatov
		/// 	},
		/// 	{
		/// 		FirstName: Ryan
		/// 		LastName: Leonard
		/// 	}
		/// ]
		/// 
		/// then for the section "Data", the resulting enumerable will contain two sections (names tuples)
		/// </summary>
		/// <param name="section">The section for which to extract content as enumerable of sections.</param>
		/// <returns>Enumerable of sections for given section.</returns>
		public static IEnumerable<IConfigurationSection> SectionsFromArray(this IConfiguration config, string section)
		{
			var sections = config.GetSection(section);

			if (
				sections == null ||
				(string.IsNullOrEmpty(config[section]) && sections.AsEnumerable().Count() == 1)
			)
			{
				return Enumerable.Empty<IConfigurationSection>();
			}

			// This snippet computes the number of sections in the array of interest.
			// Dirty but necessary.
			var count = config
				.GetSection(section)
				.AsEnumerable()
				.Count(
					pair => !pair
						.Key
						.Replace($"{section}:", "")
						.Contains(":")
				);

			// This snippet returns an enumerable of sections for the given section
			return Enumerable
				.Range(0, count)
				.Select(num => config.GetSection($"{section}:{num}"));
		}

		/// <summary>
		/// This helper method should be applied to section which contain an array of strings.
		/// In this case, the emthod returns that array of string as enumerable.
		/// </summary>
		/// <param name="section">The section for which to extract content as enumerable of strings.</param>
		/// <returns>Enumerable of strings for given section.</returns>
		public static IEnumerable<string> StringsFromArray(this IConfiguration config, string section)
		{
			return config
				.SectionsFromArray(section)
				.Select(sect => sect.Value);
		}
	}

}
