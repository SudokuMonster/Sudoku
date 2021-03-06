﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Sudoku.Extensions;
using static Sudoku.Data.CellStatus;
using static Sudoku.Constants.Processings;

namespace Sudoku.Data.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="IReadOnlyGrid"/>.
	/// </summary>
	/// <seealso cref="IReadOnlyGrid"/>
	[DebuggerStepThrough]
	public static class ReadOnlyGridEx
	{
		/// <summary>
		/// Convert the current read-only grid to mutable one.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <returns>The mutable one.</returns>
		/// <remarks>
		/// This method is only use type conversion, so the return value has a same
		/// reference with this specified argument holds.
		/// </remarks>
		/// <exception cref="InvalidCastException">
		/// Throws when <see cref="IReadOnlyGrid"/> cannot convert to a <see cref="Grid"/>.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Grid ToMutable(this IReadOnlyGrid @this) =>
			@this is Grid result
				? result
				: throw new InvalidCastException("The specified read-only grid cannot converted to a normal one.");

		/// <summary>
		/// <para>Indicates whether the specified cell is a bivalue cell.</para>
		/// <para>
		/// Note that given and modifiable cells always make this method
		/// return <see langword="false"/>.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="cellOffset">The cell offset.</param>
		/// <param name="mask">
		/// (<see langword="out"/> parameter) The result mask. The mask consists of
		/// 9 bits, where the set bits means the digit exists in this cell; otherwise,
		/// the bit will not be set.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBivalueCell(this IReadOnlyGrid @this, int cellOffset, out short mask)
		{
			if (@this.GetStatus(cellOffset) != Empty)
			{
				mask = 0;
				return false;
			}

			mask = @this.GetCandidatesReversal(cellOffset);
			return mask.CountSet() == 2;
		}

		/// <summary>
		/// <para>
		/// Indicates whether the specified region is a bi-location region
		/// for the specified digit.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="region">The region.</param>
		/// <param name="mask">
		/// (<see langword="out"/> parameter) The mask off digit appearing mask.
		/// If the region has the value cell with this digit, this value will be 0.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBilocationRegion(this IReadOnlyGrid @this, int digit, int region, out short mask)
		{
			if (@this.HasDigitValue(digit, region))
			{
				mask = 0;
				return false;
			}

			mask = @this.GetDigitAppearingMask(digit, region);
			return mask.CountSet() == 2;
		}

		/// <summary>
		/// <para>
		/// Indicates whether the specified grid contains the digit in the specified cell.
		/// </para>
		/// <para>
		/// The return value will be <see langword="true"/> if and only if
		/// the cell is empty and contains that digit.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="cellOffset">The cell offset.</param>
		/// <param name="digit">The digit.</param>
		/// <returns>
		/// A <see cref="bool"/>? value indicating that.
		/// </returns>
		/// <remarks>
		/// The cases of the return value are below:
		/// <list type="table">
		/// <item>
		/// <term><c><see langword="true"/></c></term>
		/// <description>
		/// The cell is an empty cell <b>and</b> contains the specified digit.
		/// </description>
		/// </item>
		/// <item>
		/// <term><c><see langword="false"/></c></term>
		/// <description>
		/// The cell is an empty cell <b>but doesn't</b> contain the specified digit.
		/// </description>
		/// </item>
		/// <item>
		/// <term><c><see langword="null"/></c></term>
		/// <description>The cell is <b>not</b> an empty cell.</description>
		/// </item>
		/// </list>
		/// </remarks>
		/// <example>
		/// Note that the method will return a <see cref="bool"/>?, so you should use the code
		/// <code>grid.Exists(candidate) is true</code>
		/// or
		/// <code>grid.Exists(candidate) == true</code>
		/// to decide whether a condition is true.
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool? Exists(this IReadOnlyGrid @this, int cellOffset, int digit) =>
			@this.GetStatus(cellOffset) switch
			{
				Empty => !@this[cellOffset, digit],
				_ => null
			};

		/// <summary>
		/// Checks whether the specified digit has given or modifiable values in
		/// the specified region.
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="regionOffset">The region.</param>
		/// <returns>A <see cref="bool"/> indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasDigitValue(this IReadOnlyGrid @this, int digit, int regionOffset) =>
			RegionCells[regionOffset].Any(o => @this.GetStatus(o) != Empty && @this[o] == digit);

		/// <summary>
		/// <para>
		/// Gets a mask of digit appearing in the specified region offset.
		/// </para>
		/// <para>
		/// Note that given and modifiable cells always make this method
		/// return <see langword="false"/>.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="regionOffset">The region.</param>
		/// <returns>
		/// The mask. This value consists of 9 bits, which represents all nine cells
		/// in a specified region. The mask uses 1 to make the cell 'have this digit',
		/// and 0 to make the cell 'does not have this digit'.
		/// </returns>
		public static short GetDigitAppearingMask(this IReadOnlyGrid @this, int digit, int regionOffset)
		{
			int result = 0;
			int[] cells = RegionCells[regionOffset];
			for (int i = 0, length = cells.Length; i < length; result = i != 8 ? result << 1 : result, i++)
			{
				result += (@this.Exists(cells[i], digit) is true).ToInt32();
			}

			// Now should reverse all bits. Note that this extension method
			// will be passed a ref value ('ref int', not 'int').
			result.ReverseBits();
			return (short)(result >> 23 & 511); // 23 == 32 - 9
		}

		/// <summary>
		/// <para>
		/// Gets a mask of digit appearing in the specified region offset.
		/// If the cell is not in <paramref name="map"/>, its mask will
		/// not calculated to the result.
		/// </para>
		/// <para>
		/// Note that given and modifiable cells always make this method
		/// return <see langword="false"/>.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="regionOffset">The region offset.</param>
		/// <param name="map">The grid map.</param>
		/// <returns>
		/// The mask. This value consists of 9 bits, which represents all nine cells
		/// in a specified region. The mask uses 1 to make the cell 'have this digit',
		/// and 0 to make the cell 'does not have this digit'.
		/// </returns>
		public static short GetDigitAppearingMask(this IReadOnlyGrid @this, int digit, int regionOffset, GridMap map)
		{
			int result = 0, i = 0;
			foreach (int cell in RegionCells[regionOffset])
			{
				result += (@this.Exists(cell, digit) is true && map[cell]).ToInt32();

				if (i++ != 8)
				{
					result <<= 1;
				}
			}

			result.ReverseBits();
			return (short)(result >> 23 & 511); // 23 == 32 - 9
		}

		/// <summary>
		/// <para>
		/// Gets a <see cref="GridMap"/> of cells whose input digit appearing
		/// in the specified region offset.
		/// </para>
		/// <para>
		/// Note that given and modifiable cells always make this method
		/// return <see langword="false"/>.
		/// </para>
		/// </summary>
		/// <param name="this">(<see langword="this"/> parameter) The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="regionOffset">The region.</param>
		/// <returns>The cells' map.</returns>
		public static GridMap GetDigitAppearingCells(this IReadOnlyGrid @this, int digit, int regionOffset)
		{
			var result = GridMap.Empty;
			foreach (int cell in RegionCells[regionOffset])
			{
				if (@this.Exists(cell, digit) is true)
				{
					result.Add(cell);
				}
			}

			return result;
		}
	}
}
