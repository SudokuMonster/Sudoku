﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Drawing;
using Sudoku.Extensions;
using Sudoku.Solving.Annotations;
using Sudoku.Solving.Checking;
using static Sudoku.Constants.Processings;
using static Sudoku.Data.CellStatus;
using static Sudoku.Data.ConclusionType;
using static Sudoku.Data.GridMap.InitializeOption;
using BugMultiple = Sudoku.Solving.Manual.Uniqueness.Bugs.BugMultipleTechniqueInfo;
using BugType1 = Sudoku.Solving.Manual.Uniqueness.Bugs.BugType1TechniqueInfo;
using BugType2 = Sudoku.Solving.Manual.Uniqueness.Bugs.BugType2TechniqueInfo;
using BugType3 = Sudoku.Solving.Manual.Uniqueness.Bugs.BugType3TechniqueInfo;
using BugType4 = Sudoku.Solving.Manual.Uniqueness.Bugs.BugType4TechniqueInfo;

namespace Sudoku.Solving.Manual.Uniqueness.Bugs
{
	/// <summary>
	/// Encapsulates a <b>bivalue universal grave</b> (BUG) technique searcher.
	/// </summary>
	[TechniqueDisplay("Bivalue Universal Grave")]
	public sealed class BugTechniqueSearcher : UniquenessTechniqueSearcher
	{
		/// <summary>
		/// Indicates whether the searcher should call the extended BUG checker
		/// to find all true candidates.
		/// </summary>
		private readonly bool _extended;


		/// <summary>
		/// Initializes an instance with the region maps.
		/// </summary>
		/// <param name="extended">
		/// A <see cref="bool"/> value indicating whether the searcher should call
		/// the extended BUG checker to search for all true candidates no matter how
		/// difficult searching.
		/// </param>
		public BugTechniqueSearcher(bool extended) => _extended = extended;


		/// <summary>
		/// Indicates the priority of this technique.
		/// </summary>
		public static int Priority { get; set; } = 56;

		/// <summary>
		/// Indicates whether the technique is enabled.
		/// </summary>
		public static bool IsEnabled { get; set; } = true;


		/// <inheritdoc/>
		public override void GetAll(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid)
		{
			IReadOnlyList<int> trueCandidates;
			if (_extended)
			{
				var checker = new BugChecker(grid);
				trueCandidates = checker.TrueCandidates;
			}
			else
			{
				trueCandidates = GetTrueCandidatesSimply(grid);
			}

			switch (trueCandidates.Count)
			{
				case 0:
				{
					return;
				}
				case 1:
				{
					// BUG + 1 found.
					accumulator.Add(
						new BugType1(
							conclusions: new[] { new Conclusion(Assignment, trueCandidates[0]) },
							views: new[]
							{
								new View(
									cellOffsets: null,
									candidateOffsets: new[] { (0, trueCandidates[0]) },
									regionOffsets: null,
									links: null)
							}));
					break;
				}
				default:
				{
					if (CheckSingleDigit(trueCandidates))
					{
						CheckType2(accumulator, grid, trueCandidates);
					}
					else
					{
						if (_extended)
						{
							CheckMultiple(accumulator, grid, trueCandidates);
							CheckXz(accumulator, grid, trueCandidates);
						}

						CheckType4(accumulator, grid, trueCandidates);
						for (int size = 2; size <= 5; size++)
						{
							CheckType3Naked(accumulator, grid, trueCandidates, size);
						}
					}

					break;
				}
			}
		}

		/// <summary>
		/// Check type 3 (with naked subsets).
		/// </summary>
		/// <param name="accumulator">The result.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		/// <param name="size">The size.</param>
		private void CheckType3Naked(
			IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid, IReadOnlyList<int> trueCandidates, int size)
		{
			// Check whether all true candidates lie on a same region.
			var candsGroupByCell = from cand in trueCandidates group cand by cand / 9;
			var trueCandidateCells = from candGroupByCell in candsGroupByCell
									 select candGroupByCell.Key;
			int trueCandidateCellsCount = 0;
			var map = new GridMap(trueCandidateCells, ProcessPeersAlso);
			if (map.Count != 9)
			{
				return;
			}

			foreach (var candGroupByCell in candsGroupByCell)
			{
				// Get the region.
				int region = default;
				for (int i = 0; i < 27; i++)
				{
					if (RegionMaps[i] == map)
					{
						region = i;
						break;
					}
				}

				int[] cells = RegionCells[region];
				if (cells.Count(c => grid.GetStatus(c) == Empty) - trueCandidateCellsCount <= size - 1)
				{
					continue;
				}

				short maskInTrueCandidateCells = 0;
				foreach (int cand in trueCandidates)
				{
					maskInTrueCandidateCells |= (short)(1 << cand % 9);
				}

				for (int i1 = 0; i1 < 11 - size; i1++)
				{
					int c1 = RegionCells[region][i1];
					short mask1 = grid.GetCandidatesReversal(c1);
					if (size == 2)
					{
						// Check naked pair.
						short mask = (short)(mask1 | maskInTrueCandidateCells);
						if (mask.CountSet() != 2)
						{
							continue;
						}

						// Naked pair found.
						var digits = mask.GetAllSets();

						// Record all eliminations.
						var conclusions = new List<Conclusion>();
						foreach (int cell in cells)
						{
							if (trueCandidateCells.Contains(cell) || c1 == cell)
							{
								continue;
							}

							foreach (int digit in digits)
							{
								if (!(grid.Exists(cell, digit) is true))
								{
									continue;
								}

								conclusions.Add(new Conclusion(Elimination, cell, digit));
							}
						}

						if (conclusions.Count == 0)
						{
							continue;
						}

						// Record all highlight candidates.
						var candidateOffsets = new List<(int, int)>();
						foreach (int cand in candGroupByCell)
						{
							candidateOffsets.Add((0, cand));
						}
						var digitsInCell1 = grid.GetCandidatesReversal(c1).GetAllSets();
						foreach (int digit in digitsInCell1)
						{
							candidateOffsets.Add((1, c1 * 9 + digit));
						}

						// BUG type 3 (with naked pair).
						accumulator.Add(
							new BugType3(
								conclusions,
								views: new[]
								{
									new View(
										cellOffsets: null,
										candidateOffsets,
										regionOffsets: new[] { (0, region) },
										links: null)
								},
								trueCandidates,
								digits: digits.ToArray(),
								cells: new[] { c1 },
								isNaked: true));
					}
					else // size > 2
					{
						for (int i2 = i1 + 1; i2 < 12 - size; i2++)
						{
							int c2 = RegionCells[region][i2];
							short mask2 = grid.GetCandidatesReversal(c2);
							if (size == 3)
							{
								// Check naked triple.
								short mask = (short)((short)(mask1 | mask2) | maskInTrueCandidateCells);
								if (mask.CountSet() != 3)
								{
									continue;
								}

								// Naked triple found.
								var digits = mask.GetAllSets();

								// Record all eliminations.
								var conclusions = new List<Conclusion>();
								foreach (int cell in cells)
								{
									if (trueCandidateCells.Contains(cell) || c1 == cell || c2 == cell)
									{
										continue;
									}

									foreach (int digit in digits)
									{
										if (grid.Exists(cell, digit) is true)
										{
											conclusions.Add(new Conclusion(Elimination, cell, digit));
										}
									}
								}

								if (conclusions.Count == 0)
								{
									continue;
								}

								// Record all highlight candidates.
								var candidateOffsets = new List<(int, int)>();
								foreach (int cand in candGroupByCell)
								{
									candidateOffsets.Add((0, cand));
								}
								foreach (int digit in grid.GetCandidatesReversal(c1).GetAllSets())
								{
									candidateOffsets.Add((1, c1 * 9 + digit));
								}
								foreach (int digit in grid.GetCandidatesReversal(c2).GetAllSets())
								{
									candidateOffsets.Add((1, c2 * 9 + digit));
								}

								// BUG type 3 (with naked triple).
								accumulator.Add(
									new BugType3(
										conclusions,
										views: new[]
										{
											new View(
												cellOffsets: null,
												candidateOffsets,
												regionOffsets: new[] { (0, region) },
												links: null)
										},
										trueCandidates,
										digits: digits.ToArray(),
										cells: new[] { c1, c2 },
										isNaked: true));
							}
							else // size > 3
							{
								for (int i3 = i2 + 1; i3 < 13 - size; i3++)
								{
									int c3 = RegionCells[region][i3];
									short mask3 = grid.GetCandidatesReversal(c3);
									if (size == 4)
									{
										// Check naked quadruple.
										short mask = (short)((short)((short)(mask1 | mask2) | mask3) | maskInTrueCandidateCells);
										if (mask.CountSet() != 4)
										{
											continue;
										}

										// Naked quadruple found.
										var digits = mask.GetAllSets();

										// Record all eliminations.
										var conclusions = new List<Conclusion>();
										foreach (int cell in cells)
										{
											if (trueCandidateCells.Contains(cell)
												|| c1 == cell || c2 == cell || c3 == cell)
											{
												continue;
											}

											foreach (int digit in digits)
											{
												if (grid.Exists(cell, digit) is true)
												{
													conclusions.Add(new Conclusion(Elimination, cell, digit));
												}
											}
										}

										if (conclusions.Count == 0)
										{
											continue;
										}

										// Record all highlight candidates.
										var candidateOffsets = new List<(int, int)>();
										foreach (int cand in candGroupByCell)
										{
											candidateOffsets.Add((0, cand));
										}
										foreach (int digit in grid.GetCandidatesReversal(c1).GetAllSets())
										{
											candidateOffsets.Add((1, c1 * 9 + digit));
										}
										foreach (int digit in grid.GetCandidatesReversal(c2).GetAllSets())
										{
											candidateOffsets.Add((1, c2 * 9 + digit));
										}
										foreach (int digit in grid.GetCandidatesReversal(c3).GetAllSets())
										{
											candidateOffsets.Add((1, c3 * 9 + digit));
										}

										// BUG type 3 (with naked quadruple).
										accumulator.Add(
											new BugType3(
												conclusions,
												views: new[]
												{
													new View(
														cellOffsets: null,
														candidateOffsets,
														regionOffsets: new[] { (0, region) },
														links: null)
												},
												trueCandidates,
												digits: digits.ToArray(),
												cells: new[] { c1, c2, c3 },
												isNaked: true));
									}
									else // size == 5
									{
										for (int i4 = i3 + 1; i4 < 9; i4++)
										{
											int c4 = RegionCells[region][i4];
											short mask4 = grid.GetCandidatesReversal(c4);

											// Check naked quintuple.
											short mask = (short)((short)((short)((short)
												(mask1 | mask2) | mask3) | mask4) | maskInTrueCandidateCells);
											if (mask.CountSet() != 5)
											{
												continue;
											}

											// Naked quintuple found.
											var digits = mask.GetAllSets();

											// Record all eliminations.
											var conclusions = new List<Conclusion>();
											foreach (int cell in cells)
											{
												if (trueCandidateCells.Contains(cell)
													|| c1 == cell || c2 == cell || c3 == cell || c4 == cell)
												{
													continue;
												}

												foreach (int digit in digits)
												{
													if (grid.Exists(cell, digit) is true)
													{
														conclusions.Add(new Conclusion(Elimination, cell, digit));
													}
												}
											}

											if (conclusions.Count == 0)
											{
												continue;
											}

											// Record all highlight candidates.
											var candidateOffsets = new List<(int, int)>();
											foreach (int cand in candGroupByCell)
											{
												candidateOffsets.Add((0, cand));
											}
											foreach (int digit in grid.GetCandidatesReversal(c1).GetAllSets())
											{
												candidateOffsets.Add((1, c1 * 9 + digit));
											}
											foreach (int digit in grid.GetCandidatesReversal(c2).GetAllSets())
											{
												candidateOffsets.Add((1, c2 * 9 + digit));
											}
											foreach (int digit in grid.GetCandidatesReversal(c3).GetAllSets())
											{
												candidateOffsets.Add((1, c3 * 9 + digit));
											}
											foreach (int digit in grid.GetCandidatesReversal(c4).GetAllSets())
											{
												candidateOffsets.Add((1, c4 * 9 + digit));
											}

											// BUG type 3 (with naked quintuple).
											accumulator.Add(
												new BugType3(
													conclusions,
													views: new[]
													{
														new View(
															cellOffsets: null,
															candidateOffsets,
															regionOffsets: new[] { (0, region) },
															links: null)
													},
													trueCandidates,
													digits: digits.ToArray(),
													cells: new[] { c1, c2, c3, c4 },
													isNaked: true));
										}
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Check type 4.
		/// </summary>
		/// <param name="accumulator">The result.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		private void CheckType4(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid, IReadOnlyList<int> trueCandidates)
		{
			// Conjugate pairs should lie on two cells.
			var candsGroupByCell = from cand in trueCandidates group cand by cand / 9;
			if (candsGroupByCell.Count() != 2)
			{
				return;
			}

			// Check two cell has same region.
			var cells = new List<int>();
			foreach (var candGroupByCell in candsGroupByCell)
			{
				cells.Add(candGroupByCell.Key);
			}

			var regions = new GridMap(cells).CoveredRegions;
			if (regions.None())
			{
				return;
			}

			// Check for each region.
			foreach (int region in regions)
			{
				// Add up all digits.
				var digits = new HashSet<int>();
				foreach (var candGroupByCell in candsGroupByCell)
				{
					foreach (int cand in candGroupByCell)
					{
						digits.Add(cand % 9);
					}
				}

				// Check whether exists a conjugate pair in this region.
				for (int conjuagtePairDigit = 0; conjuagtePairDigit < 9; conjuagtePairDigit++)
				{
					// Check whether forms a conjugate pair.
					short mask = grid.GetDigitAppearingMask(conjuagtePairDigit, region);
					if (mask.CountSet() != 2)
					{
						continue;
					}

					// Check whether the conjugate pair lies on current two cells.
					int c1 = RegionCells[region][mask.SetAt(0)];
					int c2 = RegionCells[region][mask.SetAt(1)];
					if (c1 != cells[0] || c2 != cells[1])
					{
						continue;
					}

					// Check whether all digits contain that digit.
					if (digits.Contains(conjuagtePairDigit))
					{
						continue;
					}

					// BUG type 4 found.
					// Now add up all eliminations.
					var conclusions = new List<Conclusion>();
					foreach (var candGroupByCell in candsGroupByCell)
					{
						int cell = candGroupByCell.Key;
						short digitMask = 0;
						foreach (int cand in candGroupByCell)
						{
							digitMask |= (short)(1 << cand % 9);
						}

						// Bitwise not.
						digitMask = (short)(~digitMask & 511);
						foreach (int d in digitMask.GetAllSets())
						{
							if (conjuagtePairDigit == d || !(grid.Exists(cell, d) is true))
							{
								continue;
							}

							conclusions.Add(new Conclusion(Elimination, cell, d));
						}
					}

					// Check eliminations.
					if (conclusions.Count == 0)
					{
						continue;
					}

					// BUG type 4.
					accumulator.Add(
						new BugType4(
							conclusions,
							views: new[]
							{
								new View(
									cellOffsets: null,
									candidateOffsets:
										new List<(int, int)>(from cand in trueCandidates select (0, cand))
										{
											(1, c1 * 9 + conjuagtePairDigit),
											(1, c2 * 9 + conjuagtePairDigit)
										},
									regionOffsets: new[] { (0, region) },
									links: null)
							},
							digits.ToList(),
							cells,
							conjugatePair: new ConjugatePair(c1, c2, conjuagtePairDigit)));
				}
			}
		}

		/// <summary>
		/// Check BUG + n.
		/// </summary>
		/// <param name="accumulator">The result list.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		private void CheckMultiple(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid, IReadOnlyList<int> trueCandidates)
		{
			if (trueCandidates.Count > 18)
			{
				return;
			}

			var digits = new List<int>();
			foreach (int cand in trueCandidates)
			{
				digits.AddIfDoesNotContain(cand % 9);
			}

			if (digits.Count > 2)
			{
				return;
			}

			var map = FullGridMap.CreateInstance(trueCandidates);
			if (map.Count == 0)
			{
				return;
			}

			// BUG + n found.
			// Check eliminations.
			var conclusions = new List<Conclusion>();
			foreach (int candidate in map)
			{
				if (!(grid.Exists(candidate / 9, candidate % 9) is true))
				{
					continue;
				}

				conclusions.Add(new Conclusion(Elimination, candidate));
			}

			if (conclusions.Count == 0)
			{
				return;
			}

			// BUG + n.
			accumulator.Add(
				new BugMultiple(
					conclusions,
					views: new[]
					{
						new View(
							cellOffsets: null,
							candidateOffsets:
								new List<(int, int)>(
									from cand in trueCandidates select (0, cand)),
							regionOffsets: null,
							links: null)
					},
					candidates: trueCandidates));
		}

		/// <summary>
		/// Check type 2.
		/// </summary>
		/// <param name="accumulator">The result list.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		private void CheckType2(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid, IReadOnlyList<int> trueCandidates)
		{
			var map = new GridMap(from cand in trueCandidates select cand / 9, ProcessPeersAlso);
			if (map.IsEmpty)
			{
				return;
			}

			// BUG type 2 found.
			// Check eliminations.
			var conclusions = new List<Conclusion>();
			int digit = trueCandidates[0] % 9;
			foreach (int cell in map)
			{
				if (!(grid.Exists(cell, digit) is true))
				{
					continue;
				}

				conclusions.Add(new Conclusion(Elimination, cell, digit));
			}

			if (conclusions.Count == 0)
			{
				return;
			}

			// BUG type 2.
			accumulator.Add(
				new BugType2(
					conclusions,
					views: new[]
					{
						new View(
							cellOffsets: null,
							candidateOffsets: new List<(int, int)>(from cand in trueCandidates select (0, cand)),
							regionOffsets: null,
							links: null)
					},
					digit,
					cells: new List<int>(from c in trueCandidates select c / 9)));
		}

		/// <summary>
		/// Check BUG-XZ.
		/// </summary>
		/// <param name="accumulator">The result list.</param>
		/// <param name="grid">The grid.</param>
		/// <param name="trueCandidates">All true candidates.</param>
		private void CheckXz(IBag<TechniqueInfo> accumulator, IReadOnlyGrid grid, IReadOnlyList<int> trueCandidates)
		{
			if (trueCandidates.Count > 2)
			{
				return;
			}

			int cand1 = trueCandidates[0], cand2 = trueCandidates[1];
			int c1 = cand1 / 9, c2 = cand2 / 9, d1 = cand1 % 9, d2 = cand2 % 9;
			short mask = (short)(1 << d1 | 1 << d2);
			foreach (int cell in ((new GridMap(c1, false) ^ new GridMap(c2, false)) & BivalueMap))
			{
				if (grid.GetCandidatesReversal(cell) != mask)
				{
					continue;
				}

				// BUG-XZ found.
				var conclusions = new List<Conclusion>();
				bool condition = new GridMap { c1, cell }.AllSetsAreInOneRegion(out _);
				int anotherCell = condition ? c2 : c1;
				int anotherDigit = condition ? d2 : d1;
				foreach (int peer in new GridMap(stackalloc[] { cell, anotherCell }, ProcessPeersWithoutItself))
				{
					if (grid.Exists(peer, anotherDigit) is true)
					{
						conclusions.Add(new Conclusion(Elimination, peer, anotherDigit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<(int, int)>(from c in trueCandidates select (0, c));
				var cellOffsets = new List<(int, int)> { (0, cell) };
				accumulator.Add(
					new BugXzTechniqueInfo(
						conclusions,
						views: new[]
						{
							new View(
								candidateOffsets,
								cellOffsets,
								regionOffsets: null,
								links: null)
						},
						digitMask: mask,
						cells: new[] { c1, c2 },
						extraCell: cell));
			}
		}

		/// <summary>
		/// To get true candidates (but simple mode).
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>All true candidates searched.</returns>
		private IReadOnlyList<int> GetTrueCandidatesSimply(IReadOnlyGrid grid)
		{
			var tempGrid = grid.Clone();
			var bugCells = new List<int>();
			var bugValues = new Dictionary<int, short>();
			short allBugValues = 0;
			var commonCells = default(GridMap);
			int t = 0;
			for (int region = 0; region < 27; region++)
			{
				for (int digit = 0; digit < 9; digit++)
				{
					// Possible positions of a value in a region.
					short positions = tempGrid.GetDigitAppearingMask(digit, region);
					int cardinality = positions.CountSet();
					if (cardinality != 0 && cardinality != 2)
					{
						// The value does not have zero or two positions
						// in the region.
						// Look for BUG cells.
						var newBugCells = new List<int>();
						foreach (int i in positions.GetAllSets())
						{
							int cell = RegionCells[region][i];
							int cellCardinality = tempGrid.GetCandidatesReversal(cell).CountSet();
							if (cellCardinality >= 3)
							{
								newBugCells.Add(cell);
							}
						}

						// If there're two or more positions falling in a BUG cell,
						// we cannot decide which one is the BUGgy one. Just do
						// nothing because another region will capture the correct
						// cell.
						if (newBugCells.Count == 1)
						{
							// A new BUG cell has been found (BUG value == 'value').
							int cell = newBugCells[0];
							bugCells.AddIfDoesNotContain(cell);
							bugValues.AddIfKeyDoesNotContain(cell, (short)0);
							short mask = (short)(1 << digit);
							bugValues[cell] |= mask;
							allBugValues |= mask;
							tempGrid[cell, digit] = true;

							if (t++ == 0)
							{
								commonCells = new GridMap(cell);
							}
							else
							{
								commonCells &= new GridMap(cell);
							}

							foreach (int bugCell in bugCells)
							{
								commonCells.Remove(bugCell);
							}

							if (bugCells.Count > 1 && allBugValues.CountSet() > 1 && commonCells.IsEmpty)
							{
								// None of type 1, 2 or 3.
								return Array.Empty<int>();
							}
						}

						if (newBugCells.Count == 0)
						{
							// A value appear more than twice, but no cell has more
							// than two values, which means that the specified pattern
							// is not a BUG pattern.
							return Array.Empty<int>();
						}
					}
				}
			}

			// When BUG values have been removed, all remaining empty cells must
			// have exactly two potential values. Now check it.
			for (int cell = 0; cell < 81; cell++)
			{
				if (tempGrid.GetStatus(cell) == Empty
					&& tempGrid.GetCandidatesReversal(cell).CountSet() != 2)
				{
					// Not a BUG.
					return Array.Empty<int>();
				}
			}

			// When BUG values have been removed, all remaining candidates must have
			// two positions in each region.
			for (int region = 0; region < 27; region++)
			{
				for (int digit = 0; digit < 9; digit++)
				{
					short mask = tempGrid.GetDigitAppearingMask(digit, region);
					int count = mask.CountSet();
					if (count != 0 && count != 2)
					{
						// Not a BUG.
						return Array.Empty<int>();
					}
				}
			}

			// Record the result.
			var result = new List<int>();
			foreach (var (cell, digitsMask) in bugValues)
			{
				foreach (int digit in digitsMask.GetAllSets())
				{
					result.Add(cell * 9 + digit);
				}
			}

			return result;
		}

		/// <summary>
		/// Check whether all candidates in the list has same digit value.
		/// </summary>
		/// <param name="list">The list of all true candidates.</param>
		/// <returns>A <see cref="bool"/> indicating that.</returns>
		private static bool CheckSingleDigit(IReadOnlyList<int> list)
		{
			int i = 0;
			int comparer = default;
			foreach (int cand in list)
			{
				if (i++ == 0)
				{
					comparer = cand % 9;
					continue;
				}

				if (comparer != cand % 9)
				{
					return false;
				}
			}

			return true;
		}
	}
}
