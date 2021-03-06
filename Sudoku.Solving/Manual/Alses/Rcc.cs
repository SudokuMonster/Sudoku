﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Constants;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Extensions;

namespace Sudoku.Solving.Manual.Alses
{
	/// <summary>
	/// Encapsulates a data structure for restricted common candidates (RCC).
	/// </summary>
	public readonly struct Rcc : IEquatable<Rcc>
	{
		/// <summary>
		/// Initializes an instance with two ALSes and their common digit.
		/// </summary>
		/// <param name="als1">The ALS 1.</param>
		/// <param name="als2">The ALS 2.</param>
		/// <param name="commonDigit">The common digit.</param>
		/// <param name="commonRegion">The common region.</param>
		public Rcc(Als als1, Als als2, int commonDigit, int commonRegion) =>
			(Als1, Als2, CommonDigit, CommonRegion) = (als1, als2, commonDigit, commonRegion);


		/// <summary>
		/// Indicates the ALS 1.
		/// </summary>
		public Als Als1 { get; }

		/// <summary>
		/// Indicates the ALS 2.
		/// </summary>
		public Als Als2 { get; }

		/// <summary>
		/// Indicates the digit that two ALSes share.
		/// </summary>
		public int CommonDigit { get; }

		/// <summary>
		/// Indicates the common region.
		/// </summary>
		public int CommonRegion { get; }


		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="Deconstruct"]'/>
		/// <param name="als1">(<see langword="out"/> parameter) The ALS 1.</param>
		/// <param name="als2">(<see langword="out"/> parameter) The ALS 2.</param>
		/// <param name="commonDigit">
		/// (<see langword="out"/> parameter) The common digit.
		/// </param>
		/// <param name="commonRegion">
		/// (<see langword="out"/> parameter) The common region.
		/// </param>
		public void Deconstruct(out Als als1, out Als als2, out int commonDigit, out int commonRegion) =>
			(als1, als2, commonDigit, commonRegion) = (Als1, Als2, CommonDigit, CommonRegion);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is Rcc comparer && Equals(comparer);

		/// <inheritdoc/>
		public bool Equals(Rcc other) =>
			CommonDigit == other.CommonDigit
			&& CommonRegion == other.CommonRegion
			&& (Als1 == other.Als1 && Als2 == other.Als2 || Als1 == other.Als2 && Als2 == other.Als1);

		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="GetHashCode"]'/>
		public override int GetHashCode() =>
			Als1.GetHashCode() ^ Als2.GetHashCode() ^ CommonDigit ^ CommonRegion;

		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="ToString" and @paramType="__noparam"]'/>
		public override string ToString() => $"{CommonDigit + 1} in {Als1} & {Als2}";


		/// <summary>
		/// Get all RCCs in the specified grid.
		/// </summary>
		/// <param name="grid">The grid to check.</param>
		/// <param name="allowOverlap">
		/// Indicates whether the specified searcher allows ALSes overlapping.
		/// </param>
		/// <returns>All RCCs searched.</returns>
		public static IEnumerable<Rcc> GetAllRccs(IReadOnlyGrid grid, bool allowOverlap)
		{
			var candMaps = TechniqueSearcher.CandMaps;
			var alses = Als.GetAllAlses(grid).ToArray();
			for (int i = 0, length = alses.Length; i < length - 1; i++)
			{
				var als1 = alses[i];
				for (int j = i + 1; j < length; j++)
				{
					var als2 = alses[j];
					if ((als1.Map | als2.Map).AllSetsAreInOneRegion(out _))
					{
						// Disallow two ALSes in the same region.
						// Here will filter the cases, where the ALS is in the mini-row
						// or mini-column.
						continue;
					}

					// Check whether two ALSes hold same cells.
					foreach (var (commonDigit, region) in GetCommonDigits(grid, als1, als2, out short digitsMask))
					{
						var overlapMap = als1.Map & als2.Map;
						if (allowOverlap && overlapMap.IsNotEmpty && overlapMap.Overlaps(candMaps[commonDigit]))
						{
							continue;
						}

						// Now we should check elimination.
						// But firstly, we should check all digits appearing
						// in two ALSes.
						foreach (int elimDigit in (digitsMask & ~(1 << commonDigit)).GetAllSets())
						{
							yield return new Rcc(als1, als2, commonDigit, region);
						}
					}
				}
			}
		}

		/// <summary>
		/// Get the common digit that two ALSes share.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="als1">The ALS 1.</param>
		/// <param name="als2">The ALS 2.</param>
		/// <param name="digitsMask">
		/// (<see langword="out"/> parameter) The mask of appearing digits.
		/// </param>
		/// <returns>
		/// The digit. If the method cannot find out a digit,
		/// it will return <see langword="null"/>.
		/// </returns>
		public static IEnumerable<(int _digit, int _region)> GetCommonDigits(
			IReadOnlyGrid grid, Als als1, Als als2, out short digitsMask)
		{
			var result = new List<(int, int)>();
			foreach (int digit in (digitsMask = (short)(als1.DigitsMask & als2.DigitsMask)).GetAllSets())
			{
				if (DigitAppears(grid, als1, digit, out var map1)
					&& DigitAppears(grid, als2, digit, out var map2)
					&& (map1 | map2).AllSetsAreInOneRegion(out int region))
				{
					result.Add((digit, region));
				}
			}

			return result;
		}

		/// <summary>
		/// Check whether the digit appears at least once in the specified ALS.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="als">The ALS.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="map">
		/// (<see langword="out"/> parameter) The map of cells.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		private static bool DigitAppears(IReadOnlyGrid grid, Als als, int digit, out GridMap map)
		{
			map = GridMap.Empty;
			foreach (int cell in als.Cells)
			{
				if (grid.Exists(cell, digit) is true)
				{
					map.Add(cell);
				}
			}

			return map.IsNotEmpty;
		}


		/// <include file='../GlobalDocComments.xml' path='comments/operator[@name="op_Equality"]'/>
		public static bool operator ==(Rcc left, Rcc right) => left.Equals(right);

		/// <include file='../GlobalDocComments.xml' path='comments/operator[@name="op_Inequality"]'/>
		public static bool operator !=(Rcc left, Rcc right) => !(left == right);
	}
}
