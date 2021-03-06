﻿using System.Collections.Generic;
using Sudoku.Data.Collections;
using static Sudoku.Solving.Constants.Processings;

namespace Sudoku.Solving.Manual.Uniqueness.Loops
{
	/// <summary>
	/// Indicates the detail data of UL type 3.
	/// </summary>
	public sealed class UlType3DetailData : UlDetailData
	{
		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="cells">All cells.</param>
		/// <param name="digits">All digits.</param>
		/// <param name="subsetDigits">All subset digits.</param>
		/// <param name="subsetCells">All subset cells.</param>
		/// <param name="isNaked">Indicates whether the subset is naked or not.</param>
		public UlType3DetailData(
			IReadOnlyList<int> cells, IReadOnlyList<int> digits,
			IReadOnlyList<int> subsetDigits, IReadOnlyList<int> subsetCells, bool isNaked)
			: base(cells, digits) =>
			(SubsetDigits, SubsetCells, IsNaked) = (subsetDigits, subsetCells, isNaked);


		/// <inheritdoc/>
		public override int Type => 3;

		/// <summary>
		/// Indicates all subset digits in this pattern.
		/// </summary>
		public IReadOnlyList<int> SubsetDigits { get; }

		/// <summary>
		/// Indicates all subset cells in this pattern.
		/// </summary>
		public IReadOnlyList<int> SubsetCells { get; }

		/// <summary>
		/// Indicates whether this subset is naked or not.
		/// </summary>
		public bool IsNaked { get; }


		/// <inheritdoc/>
		public override string ToString()
		{
			string cellsStr = new CellCollection(Cells).ToString();
			string digitsStr = new DigitCollection(Digits).ToString();
			string subsetDigitsStr = new DigitCollection(SubsetDigits).ToString();
			string subsetCellsStr = new CellCollection(SubsetCells).ToString();
			string subsetType = IsNaked ? "naked" : "hidden";
			string subsetName = SubsetNames[SubsetCells.Count + 1].ToLower();
			return
				$"{digitsStr} in cells {cellsStr} with {subsetType} {subsetName}: " +
				$"digits {subsetDigitsStr} in cells {subsetCellsStr}";
		}
	}
}
