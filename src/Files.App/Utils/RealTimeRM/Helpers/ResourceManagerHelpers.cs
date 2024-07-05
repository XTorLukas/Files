// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Utils.RealTimeRM.Helpers
{
	/// <summary>
	/// Provides helper methods for managing resources.
	/// </summary>
	public static class ResourceManagerHelpers
	{
		/// <summary>
		/// The default culture name used when no specific culture is specified.
		/// </summary>
		public static readonly string DefaultCultureName = "en-US";

		/// <summary>
		/// The character used to separate parts of manifest resource names.
		/// </summary>
		public static readonly char ManifestSeparatorChar = '.';

		/// <summary>
		/// The character used to separate culture names in standard resource file names.
		/// </summary>
		public static readonly char CultureSeparatorChar = '-';

		/// <summary>
		/// The character used to separate culture names in manifest resource file names.
		/// </summary>
		public static readonly char LocaleSeparatorChar = '_';

		/// <summary>
		/// Combines two parts of a path into a single string, using the specified separator.
		/// </summary>
		/// <param name="buffer">The buffer where the combined path will be stored.</param>
		/// <param name="part1">The first part of the path.</param>
		/// <param name="part2">The second part of the path.</param>
		/// <param name="separator">The character to use as the separator between the parts.</param>
		/// <returns>The length of the combined path.</returns>
		public static int CombinePath(Span<char> buffer, ReadOnlySpan<char> part1, ReadOnlySpan<char> part2, char separator)
		{
			var length = part1.Length;
			part1.CopyTo(buffer);
			buffer[length++] = separator;
			part2.CopyTo(buffer[length..]);
			length += part2.Length;
			return length;
		}

		/// <summary>
		/// Replaces all occurrences of the directory separator character in a given span of characters with a specified replacement character.
		/// </summary>
		/// <param name="buffer">The span of characters where the replacement will be performed.</param>
		/// <param name="replacement">The character to replace the directory separator character with.</param>
		/// <returns>The length of the modified span. Since no characters are added or removed, the length remains the same.</returns>
		public static int ReplacePathSeparator(Span<char> buffer, char replacement)
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				// If the current character is a directory separator, replace it with the replacement character.
				if (buffer[i] == SystemIO.Path.DirectorySeparatorChar)
				{
					buffer[i] = replacement;
				}
			}

			// Return the length of the modified span. Since no characters are added or removed, the length remains the same.
			return buffer.Length;
		}
	}
}
