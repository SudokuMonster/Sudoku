﻿using System;
using System.Collections.Generic;
using Sudoku.Constants;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Intersections
{
	/// <summary>
	/// Provides a usage of <b>almost locked candidates</b> (ALC) technique.
	/// </summary>
	public sealed class AlcTechniqueInfo : IntersectionTechniqueInfo
	{
		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="conclusions">All conclusions.</param>
		/// <param name="views">All views.</param>
		/// <param name="digits">The digits.</param>
		/// <param name="baseCells">The base cells.</param>
		/// <param name="targetCells">The target cells.</param>
		/// <param name="hasValueCell">
		/// Indicates whether the structure has the value cell.
		/// </param>
		public AlcTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			IReadOnlyList<int> digits, IReadOnlyList<int> baseCells, IReadOnlyList<int> targetCells,
			bool hasValueCell) : base(conclusions, views) =>
			(Digits, BaseCells, TargetCells, HasValueCell) = (digits, baseCells, targetCells, hasValueCell);


		/// <summary>
		/// Indicates the digits the technique used.
		/// </summary>
		public IReadOnlyList<int> Digits { get; }

		/// <summary>
		/// Indicates the base cells.
		/// </summary>
		public IReadOnlyList<int> BaseCells { get; }

		/// <summary>
		/// Indicates the target cells.
		/// </summary>
		public IReadOnlyList<int> TargetCells { get; }

		/// <summary>
		/// Indicates whether the structure has a value cell.
		/// </summary>
		public bool HasValueCell { get; }

		/// <summary>
		/// Indicates the size.
		/// </summary>
		public int Size => Digits.Count;

		/// <inheritdoc/>
		public override decimal Difficulty =>
			Size switch { 2 => 4.5M, 3 => 5.2M, 4 => 5.7M, _ => throw Throwings.ImpossibleCase }
			+ (HasValueCell ? Size switch { 2 => .1M, 3 => .1M, 4 => .2M, _ => throw Throwings.ImpossibleCase } : 0);

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode =>
			Size switch
			{
				2 => TechniqueCode.AlmostLockedPair,
				3 => TechniqueCode.AlmostLockedTriple,
				4 => TechniqueCode.AlmostLockedQuadruple,
				_ => throw new NotSupportedException("The current instance does not support.")
			};


		/// <inheritdoc/>
		public override string ToString()
		{
			string digitsStr = new DigitCollection(Digits).ToString();
			string baseCellsStr = new CellCollection(BaseCells).ToString();
			string targetCellsStr = new CellCollection(TargetCells).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: {digitsStr} in {baseCellsStr} to {targetCellsStr} => {elimStr}";
		}
	}
}
