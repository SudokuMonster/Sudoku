﻿using System.Collections.Generic;
using Sudoku.Constants;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;

namespace Sudoku.Solving.Manual.Subsets
{
	/// <summary>
	/// Provides a usage of <b>hidden subset</b> technique.
	/// </summary>
	public sealed class HiddenSubsetTechniqueInfo : SubsetTechniqueInfo
	{
		/// <inheritdoc/>
		public HiddenSubsetTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int regionOffset, IReadOnlyList<int> cellOffsets, IReadOnlyList<int> digits)
			: base(conclusions, views, regionOffset, cellOffsets, digits)
		{
		}


		/// <inheritdoc/>
		public override decimal Difficulty =>
			Size switch
			{
				2 => 3.4M,
				3 => 4M,
				4 => 5.4M,
				_ => throw Throwings.ImpossibleCase
			};

		/// <summary>
		/// Indicates the size of this instance.
		/// </summary>
		public int Size => Digits.Count;

		/// <inheritdoc/>
		public override TechniqueCode TechniqueCode =>
			Size switch
			{
				2 => TechniqueCode.HiddenPair,
				3 => TechniqueCode.HiddenTriple,
				4 => TechniqueCode.HiddenQuadruple,
				_ => throw Throwings.ImpossibleCase
			};


		/// <inheritdoc/>
		public override string ToString()
		{
			string digitsStr = new DigitCollection(Digits).ToString();
			string regionStr = new RegionCollection(RegionOffset).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: {digitsStr} in {regionStr} => {elimStr}";
		}
	}
}
