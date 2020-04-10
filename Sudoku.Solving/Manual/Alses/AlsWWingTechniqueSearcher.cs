﻿using System.Collections.Generic;
using System.Linq;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Extensions;
using Sudoku.Solving.Utils;
using static Sudoku.GridProcessings;

namespace Sudoku.Solving.Manual.Alses
{
	/// <summary>
	/// Encapsulates an <b>almost locked sets W-Wing</b> technique.
	/// </summary>
	[TechniqueDisplay("Almost Locked Sets W-Wing")]
	public sealed class AlsWWingTechniqueSearcher : AlsTechniqueSearcher
	{
		/// <summary>
		/// Indicates whether the ALSes can be overlapped with each other.
		/// </summary>
		private readonly bool _allowOverlapping;

		/// <summary>
		/// Indicates whether the ALSes shows their region rather than cells.
		/// </summary>
		private readonly bool _alsShowRegions;


		/// <summary>
		/// Initialize an instance with the specified information.
		/// </summary>
		/// <param name="allowOverlapping">
		/// Indicates whether the ALSes can be overlapped with each other.
		/// </param>
		/// <param name="alsShowRegions">
		/// Indicates whether all ALSes shows their regions rather than cells.
		/// </param>
		public AlsWWingTechniqueSearcher(bool allowOverlapping, bool alsShowRegions) =>
			(_allowOverlapping, _alsShowRegions) = (allowOverlapping, alsShowRegions);


		/// <summary>
		/// Indicates the priority of this technique.
		/// </summary>
		public static int Priority { get; set; } = 58;

		/// <summary>
		/// Indicates whether the technique is enabled.
		/// </summary>
		public static bool IsEnabled { get; set; } = true;


		/// <inheritdoc/>
		public override void AccumulateAll(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid)
		{
			(_, _, var digitDistributions) = grid;
			for (int r1 = 0; r1 < 26; r1++)
			{
				if (RegionCells[r1].All(c => grid.GetCellStatus(c) != CellStatus.Empty))
				{
					continue;
				}

				var alses1 = Als.GetAllAlses(grid, r1);
				if (!alses1.Any())
				{
					continue;
				}

				for (int r2 = r1 + 1; r2 < 27; r2++)
				{
					if (RegionCells[r2].All(c => grid.GetCellStatus(c) != CellStatus.Empty))
					{
						continue;
					}

					var alses2 = Als.GetAllAlses(grid, r2);
					if (!alses2.Any())
					{
						continue;
					}

					foreach (var als1 in alses1)
					{
						var als1Cells = als1.Map.Offsets;
						foreach (var als2 in alses2)
						{
							var als2Cells = als2.Map.Offsets;
							foreach (short mask1 in als1.StrongLinksMask)
							{
								foreach (short mask2 in als2.StrongLinksMask)
								{
									if (mask1 != mask2)
									{
										continue;
									}

									// We checked the mask, and got the W and X digit.
									// Then we should check the conjugate pair, where this instance
									// should satisfy the following conditions:
									// 1) Both two ALSes contains the W and X digit.
									// 2) The region of the conjugate pair should not the same as
									//    neither one of ALS1 nor ALS2.
									// 3) The conjugate pair should be about the X digit.
									// 4) Both two endpoints of the conjugate pair should lie on
									//    the same region of all cells containing X digit in
									//    ALS1 and ALS2.
									int a = mask1.FindFirstSet();
									int b = mask1.GetNextSet(a);
									int[,] cases = new int[2, 2] { { a, b }, { b, a } };
									for (int i = 0; i < 2; i++)
									{
										int w = cases[i, 0], x = cases[i, 1];
										if ((digitDistributions[w] & als1.Map).IsEmpty
											|| (digitDistributions[x] & als1.Map).IsEmpty
											|| (digitDistributions[w] & als2.Map).IsEmpty
											|| (digitDistributions[x] & als2.Map).IsEmpty)
										{
											// Condition #1.
											continue;
										}

										for (int region = 0; region < 27; region++)
										{
											if (region == r1 || region == r2)
											{
												// Condition #2.
												continue;
											}

											short mask = grid.GetDigitAppearingMask(x, region);
											if (mask.CountSet() != 2)
											{
												// Condition #3.
												continue;
											}

											foreach (int xRegion1 in
												new GridMap(
													from cell in als1Cells
													where grid.CandidateExists(cell, x)
													select cell).CoveredRegions)
											{
												foreach (int xRegion2 in
													new GridMap(
														from cell in als2Cells
														where grid.CandidateExists(cell, x)
														select cell).CoveredRegions)
												{
													int pos = mask.FindFirstSet();
													int c1 = RegionUtils.GetCellOffset(region, pos);
													int c2 = RegionUtils.GetCellOffset(region, mask.GetNextSet(pos));
													var (row1, column1, block1) = CellUtils.GetRegion(c1);
													var (row2, column2, block2) = CellUtils.GetRegion(c2);
													if (!(
														(row1 + 9 == xRegion1 || column1 + 18 == xRegion1 || block1 == xRegion1)
														&& (row2 + 9 == xRegion2 || column2 + 18 == xRegion2 || block2 == xRegion2)
														|| (row1 + 9 == xRegion2 || column1 + 18 == xRegion2 || block1 == xRegion2)
														&& (row2 + 9 == xRegion1 || column2 + 18 == xRegion1 || block2 == xRegion1))
														|| als1.Map[c1] || als1.Map[c2] || als2.Map[c1] || als2.Map[c2])
													{
														// Condition #4.
														continue;
													}

													// Now check eliminations.
													var conclusions = new List<Conclusion>();
													var elimMap =
														new GridMap(
															from cell in (als1.Map | als2.Map).Offsets
															where grid.CandidateExists(cell, w)
															select cell,
															GridMap.InitializeOption.ProcessPeersWithoutItself);
													if (elimMap.Count == 0)
													{
														continue;
													}

													foreach (int cell in elimMap.Offsets)
													{
														if (!grid.CandidateExists(cell, w))
														{
															continue;
														}

														conclusions.Add(new Conclusion(ConclusionType.Elimination, cell, w));
													}
													if (conclusions.Count == 0)
													{
														continue;
													}

													if (!_allowOverlapping && (als1.Map & als2.Map).IsNotEmpty)
													{
														continue;
													}

													// Record all highlight elements.
													var cellOffsets = new List<(int, int)>();
													var candidateOffsets = new List<(int, int)>
													{
														(0, c1 * 9 + x),
														(0, c2 * 9 + x)
													};
													var regionOffsets = new List<(int, int)>
													{
														(-1, r1),
														(-2, r2),
														(0, region)
													};
													foreach (int cell in als1Cells)
													{
														foreach (int digit in grid.GetCandidatesReversal(cell).GetAllSets())
														{
															candidateOffsets.Add((
																true switch
																{
																	_ when digit == w => 0,
																	_ when digit == x => 1,
																	_ => -1
																}, cell * 9 + digit));
														}

														cellOffsets.Add((-1, cell));
													}
													foreach (int cell in als2Cells)
													{
														foreach (int digit in grid.GetCandidatesReversal(cell).GetAllSets())
														{
															candidateOffsets.Add((
																true switch
																{
																	_ when digit == w => 0,
																	_ when digit == x => 1,
																	_ => -2
																}, cell * 9 + digit));
														}

														cellOffsets.Add((-2, cell));
													}

													accumulator.Add(
														new AlsWWingTechniqueInfo(
															conclusions,
															views: new[]
															{
																new View(
																	cellOffsets: _alsShowRegions ? null : cellOffsets,
																	candidateOffsets: _alsShowRegions ? candidateOffsets : null,
																	regionOffsets: _alsShowRegions ? regionOffsets: null,
																	links: null)
															},
															als1,
															als2,
															w,
															x,
															conjugatePair: new ConjugatePair(c1, c2, x)));
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
