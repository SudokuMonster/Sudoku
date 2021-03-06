﻿using System.Collections.Generic;
using System.Linq;
using Sudoku.Constants;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using static Sudoku.Constants.Processings;
using static Sudoku.Data.ConclusionType;

namespace Sudoku.Solving.Manual.Wings.Irregular
{
	/// <summary>
	/// Encapsulates an <b>irregular wing</b> technique searcher.
	/// </summary>
	[TechniqueDisplay("Irregular Wing")]
	public sealed class IrregularWingTechniqueSearcher : TechniqueSearcher
	{
		/// <summary>
		/// Indicates the priority of this technique.
		/// </summary>
		public static int Priority { get; set; } = 44;

		/// <summary>
		/// Indicates whether the technique is enabled.
		/// </summary>
		public static bool IsEnabled { get; set; } = true;


		/// <inheritdoc/>
		public override void GetAll(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid)
		{
			// Finally search all irregular wings.
			// Hybrid-Wings, Local-Wings, Split-Wings and M-Wings can
			// be found in another searcher.
			// These wings are not elementary and necessary techniques
			// so we does not need to list them.
			TakeAllWWings(accumulator, grid);
		}

		/// <summary>
		/// Search for all W-Wings.
		/// </summary>
		/// <param name="result">The result accumulator.</param>
		/// <param name="grid">The grid.</param>
		/// <returns>All technique information instances.</returns>
		public static void TakeAllWWings(IBag<TechniqueInfo> result, IReadOnlyGrid grid)
		{
			if (BivalueMap.Count < 2)
			{
				return;
			}

			// Iterate on each cells.
			for (int c1 = 0; c1 < 72; c1++)
			{
				if (!BivalueMap[c1] || !EmptyMap[c1])
				{
					continue;
				}

				// Iterate on each cells which are not peers in 'c1'.
				int[] digits = grid.GetCandidatesReversal(c1).GetAllSets().ToArray();
				foreach (int c2 in BivalueMap - new GridMap(c1))
				{
					if (c2 < c1 || grid.GetCandidatesReversal(c1) != grid.GetCandidatesReversal(c2))
					{
						continue;
					}

					var intersection = new GridMap(c1, false) & new GridMap(c2, false);
					if (!EmptyMap.Overlaps(intersection))
					{
						continue;
					}

					for (int region = 9; region < 27; region++)
					{
						if (region < 18 && (
							GetRegion(c1, RegionLabel.Row) == region || GetRegion(c2, RegionLabel.Row) == region)
							|| region >= 18 && (
							GetRegion(c1, RegionLabel.Column) == region || GetRegion(c2, RegionLabel.Column) == region))
						{
							continue;
						}

						SearchWWingByRegions(result, grid, digits, region, c1, c2, intersection);
					}
				}
			}
		}

		/// <summary>
		/// Searches W-Wing technique by region.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="digits">The digits.</param>
		/// <param name="region">The region.</param>
		/// <param name="c1">Cell 1.</param>
		/// <param name="c2">Cell 2.</param>
		/// <param name="intersection">The intersection.</param>
		private static void SearchWWingByRegions(
			IBag<TechniqueInfo> result, IReadOnlyGrid grid, int[] digits, int region,
			int c1, int c2, GridMap intersection)
		{
			for (int i = 0; i < 2; i++)
			{
				int digit = digits[i];
				if (!grid.IsBilocationRegion(digit, region, out short mask))
				{
					continue;
				}

				int pos1 = mask.FindFirstSet(), pos2 = mask.GetNextSet(pos1);
				int bridgeStart = RegionCells[region][pos1], bridgeEnd = RegionCells[region][pos2];
				if (c1 == bridgeStart || c2 == bridgeStart || c1 == bridgeEnd || c2 == bridgeEnd)
				{
					continue;
				}

				static bool c(int c1, int c2) =>
					(new GridMap(c1, false) & new GridMap(c2, false)).AllSetsAreInOneRegion(out _);
				if (!(c(bridgeStart, c1) && c(bridgeEnd, c2)) && !(c(bridgeStart, c2) && c(bridgeEnd, c1)))
				{
					continue;
				}

				// W-Wing found.
				var conclusions = new List<Conclusion>();
				int elimDigit = i == 0 ? digits[1] : digits[0];
				foreach (int offset in intersection)
				{
					if (grid.Exists(offset, elimDigit) is true)
					{
						conclusions.Add(new Conclusion(Elimination, offset, elimDigit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				result.Add(
					new WWingTechniqueInfo(
						conclusions,
						views: new[]
						{
							new View(
								cellOffsets: null,
								candidateOffsets: new List<(int, int)>
								{
									(1, c1 * 9 + elimDigit),
									(0, c1 * 9 + digit),
									(0, bridgeStart * 9 + digit),
									(0, bridgeEnd * 9 + digit),
									(0, c2 * 9 + digit),
									(1, c2 * 9 + elimDigit)
								},
								regionOffsets: new[] { (0, region) },
								links: null)
						},
						startCellOffset: c1,
						endCellOffset: c2,
						conjugatePair: new ConjugatePair(bridgeStart, bridgeEnd, digit)));
			}
		}
	}
}
