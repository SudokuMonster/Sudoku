﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using static Sudoku.Constants.Processings;
using static Sudoku.Data.ConclusionType;

namespace Sudoku.Solving.Manual.Singles
{
	/// <summary>
	/// Encapsulates a <b>single</b> technique searcher.
	/// </summary>
	[TechniqueDisplay("Singles")]
	public sealed class SingleTechniqueSearcher : TechniqueSearcher
	{
		/// <summary>
		/// Indicates the solver enables these options.
		/// </summary>
		private readonly bool _enableFullHouse, _enableLastDigit;


		/// <summary>
		/// Initializes an instance with enable options.
		/// </summary>
		/// <param name="enableFullHouse">
		/// Indicates whether the solver enables full house.
		/// </param>
		/// <param name="enableLastDigit">
		/// Indicates whether the solver enables last digit.
		/// </param>
		public SingleTechniqueSearcher(bool enableFullHouse, bool enableLastDigit) =>
			(_enableFullHouse, _enableLastDigit) = (enableFullHouse, enableLastDigit);


		/// <summary>
		/// Indicates the priority of this technique.
		/// </summary>
		public static int Priority { get; set; } = 10;

		/// <summary>
		/// Indicates whether the technique is enabled.
		/// </summary>
		public static bool IsEnabled { get; set; } = true;


		/// <inheritdoc/>
		public override void GetAll(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid)
		{
			// Search for full houses.
			// Note that this technique searcher will be used in other functions,
			// so we should not use base maps like 'EmptyMap'.
			// Those maps will be initialized in the special cases.
			if (_enableFullHouse)
			{
				for (int region = 0; region < 27; region++)
				{
					var map = RegionMaps[region];
					int count = 0;
					bool flag = true;
					int resultCell = -1;
					foreach (int cell in map)
					{
						if (grid.GetStatus(cell) == CellStatus.Empty)
						{
							resultCell = cell;
							if (++count > 1)
							{
								flag.Flip();
								break;
							}
						}
					}
					if (!flag || count != 1)
					{
						continue;
					}

					int digit = grid.GetCandidatesReversal(resultCell).FindFirstSet();
					accumulator.Add(
						new FullHouseTechniqueInfo(
							conclusions: new[] { new Conclusion(Assignment, resultCell, digit) },
							views: new[]
							{
								new View(
									cellOffsets: null,
									candidateOffsets: new[] { (0, resultCell * 9 + digit) },
									regionOffsets: new[] { (0, region) },
									links: null)
							},
							cellOffset: resultCell,
							digit));
				}
			}

			// Search for hidden singles & last digits.
			for (int digit = 0; digit < 9; digit++)
			{
				for (int region = 0; region < 27; region++)
				{
					var map = RegionMaps[region];
					int count = 0, resultCell = -1;
					bool flag = true;
					foreach (int cell in map)
					{
						if (grid.Exists(cell, digit) is true)
						{
							resultCell = cell;
							if (++count > 1)
							{
								flag.Flip();
								break;
							}
						}
					}
					if (!flag || count != 1)
					{
						continue;
					}

					bool enableAndIsLastDigit = false;
					var cellOffsets = new List<(int, int)>();
					if (_enableLastDigit)
					{
						// Sum up the number of appearing in the grid of 'digit'.
						int digitCount = 0;
						for (int i = 0; i < 81; i++)
						{
							if (grid[i] == digit)
							{
								digitCount++;
								cellOffsets.Add((0, i));
							}
						}

						enableAndIsLastDigit = digitCount == 8;
					}

					accumulator.Add(
						new HiddenSingleTechniqueInfo(
							conclusions: new[] { new Conclusion(Assignment, resultCell, digit) },
							views: new[]
							{
								new View(
									cellOffsets: enableAndIsLastDigit ? cellOffsets : null,
									candidateOffsets: new[] { (0, resultCell * 9 + digit) },
									regionOffsets: enableAndIsLastDigit ? null : new[] { (0, region) },
									links: null)
							},
							regionOffset: region,
							cellOffset: resultCell,
							digit,
							enableAndIsLastDigit));
				}
			}

			// Search for naked singles.
			foreach (int cell in EmptyMap)
			{
				short mask = grid.GetCandidatesReversal(cell);
				if (mask.IsPowerOfTwo())
				{
					int digit = mask.FindFirstSet();
					accumulator.Add(
						new NakedSingleTechniqueInfo(
							conclusions: new[] { new Conclusion(Assignment, cell, digit) },
							views: new[]
							{
								new View(
									cellOffsets: null,
									candidateOffsets: new[] { (0, cell * 9 + digit) },
									regionOffsets: null,
									links: null)
							},
							cellOffset: cell,
							digit));
				}
			}
		}
	}
}
