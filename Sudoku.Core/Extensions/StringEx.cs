﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Sudoku.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="string"/>.
	/// </summary>
	/// <seealso cref="string"/>
	[DebuggerStepThrough]
	public static class StringEx
	{
		/// <summary>
		/// Check whether the specified string instance is satisfied
		/// the specified regular expression pattern or not.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to check.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		public static bool SatisfyPattern(this string @this, string pattern)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			string? match = @this.Match(pattern);
			return !(match is null) && match == @this;
		}

		/// <summary>
		/// Check whether the specified string instance can match the value
		/// using the specified regular expression pattern or not.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to match.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <returns>A <see cref="bool"/> indicating that.</returns>
		/// <remarks>
		/// This method is a syntactic sugar of the calling
		/// method <see cref="Regex.IsMatch(string, string)"/>.
		/// </remarks>
		/// <seealso cref="Regex.IsMatch(string, string)"/>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		public static bool IsMatch(this string @this, string pattern)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			return Regex.IsMatch(@this, pattern);
		}

		/// <summary>
		/// Searches the specified input string for the first occurrence of
		/// the specified regular expression pattern.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to match.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <returns>
		/// The value after matching. If failed to match,
		/// the value will be <see langword="null"/>.
		/// </returns>
		/// <remarks>
		/// This method is a syntactic sugar of the calling
		/// method <see cref="Regex.Match(string, string)"/>.
		/// </remarks>
		/// <seealso cref="Regex.Match(string, string)"/>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		public static string? Match(this string @this, string pattern)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			return @this.Match(pattern, RegexOptions.None);
		}

		/// <summary>
		/// Searches the input string for the first occurrence of the specified regular
		/// expression, using the specified matching options.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to match.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <param name="regexOption">The matching options.</param>
		/// <returns>
		/// The matched string value. If failed to match,
		/// the value will be <see langword="null"/>.
		/// </returns>
		/// <remarks>
		/// This method is a syntactic sugar of the calling
		/// method <see cref="Regex.Match(string, string, RegexOptions)"/>.
		/// </remarks>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		/// <seealso cref="Regex.Match(string, string, RegexOptions)"/>
		public static string? Match(this string @this, string pattern, RegexOptions regexOption)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			var match = Regex.Match(@this, pattern, regexOption);
			return match.Success ? match.Value : null;
		}

		/// <summary>
		/// Searches the specified input string for all occurrences of a
		/// specified regular expression pattern.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to match.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <returns>
		/// The result after matching. If failed to match,
		/// the returning array will be an empty string array (has no elements).
		/// </returns>
		/// <remarks>
		/// This method is a syntactic sugar of the calling
		/// method <see cref="Regex.Matches(string, string)"/>.
		/// </remarks>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		/// <seealso cref="Regex.Matches(string, string)"/>
		public static string[] MatchAll(this string @this, string pattern)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			return @this.MatchAll(pattern, RegexOptions.None);
		}

		/// <summary>
		/// Searches the specified input string for all occurrences of a
		/// specified regular expression pattern, using the specified matching
		/// options.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to match.</param>
		/// <param name="pattern">The regular expression pattern.</param>
		/// <param name="regexOption">The matching options.</param>
		/// <returns>
		/// The result after matching. If failed to match,
		/// the returning array will be an empty string array (has no elements).
		/// </returns>
		/// <remarks>
		/// This method is a syntactic sugar of the calling
		/// method <see cref="Regex.Matches(string, string, RegexOptions)"/>.
		/// </remarks>
		/// <exception cref="InvalidRegexStringException">
		/// Throws when the specified <paramref name="pattern"/> is not an valid regular
		/// expression pattern.
		/// </exception>
		/// <seealso cref="Regex.Matches(string, string, RegexOptions)"/>
		public static string[] MatchAll(
			this string @this, string pattern, RegexOptions regexOption)
		{
			if (!pattern.IsRegexPattern())
			{
				throw new InvalidRegexStringException();
			}

			var matches = Regex.Matches(@this, pattern, regexOption);
			var result = new List<string>();
			foreach (Match? match in matches) // Do not use 'var' ('var' is 'object?').
			{
				if (!(match is null))
				{
					result.Add(match.Value);
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// To check if the current string value is a valid regular
		/// expression pattern or not.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The value to check.</param>
		/// <returns>A <see cref="bool"/> indicating that.</returns>
		public static bool IsRegexPattern(this string @this)
		{
			try
			{
				Regex.Match(string.Empty, @this);
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
	}
}
