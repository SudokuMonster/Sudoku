﻿using System.Text;
using Sudoku.Data;
using Sudoku.Data.Extensions;

namespace Sudoku.Solving.Generating
{
	/// <summary>
	/// Provides an extended puzzle generator.
	/// </summary>
	public sealed class ExtendedPuzzleGenerator : DiggingPuzzleGenerator
	{
		/// <inheritdoc/>
		public override IReadOnlyGrid Generate()
		{
			var puzzle = new StringBuilder() { Length = 81 };
			var solution = new StringBuilder() { Length = 81 };
			var emptyGridStr = new StringBuilder(Grid.EmptyString);
			while (true)
			{
				emptyGridStr.CopyTo(puzzle);
				emptyGridStr.CopyTo(solution);
				GenerateAnswerGrid(puzzle, solution);

				int[] holeCells = new int[81];
				CreatePattern(holeCells);
				for (int trial = 0; trial < 1000; trial++)
				{
					for (int cell = 0; cell < 81; cell++)
					{
						int p = holeCells[cell];
						char temp = solution[p];
						solution[p] = '0';

						if (!FastSolver.CheckValidity(solution.ToString(), out _))
						{
							// Reset the value.
							solution[p] = temp;
						}
					}

					if (FastSolver.CheckValidity(solution.ToString(), out _))
					{
						return Grid.Parse(solution.ToString());
					}

					RecreatePattern(holeCells);
				}
			}
		}

		/// <inheritdoc/>
		protected override void CreatePattern(int[] pattern)
		{
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void swap<T>(ref T left, ref T right) { var temp = left; left = right; right = temp; }
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static double rnd() => Rng.NextDouble();
			int[] box = { 0, 6, 54, 60, 3, 27, 33, 57, 30 };
			int[,] t = { { 0, 1, 2 }, { 0, 2, 1 }, { 1, 0, 2 }, { 1, 2, 0 }, { 2, 0, 1 }, { 2, 1, 0 } };

			int a = 54, b = 0;
			for (int i = 0; i < 9; i++)
			{
				int n = (int)(rnd() * 6);
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						pattern[(k == t[n, j] ? ref a : ref b)++] = box[i] + j * 9 + k;
					}
				}
			}

			for (int i = 23; i >= 0; i--)
			{
				swap(ref pattern[i], ref pattern[(int)((i + 1) * rnd())]);
			}
			for (int i = 47; i >= 24; i--)
			{
				swap(ref pattern[i], ref pattern[24 + (int)((i - 23) * rnd())]);
			}
			for (int i = 53; i >= 48; i--)
			{
				swap(ref pattern[i], ref pattern[48 + (int)((i - 47) * rnd())]);
			}
			for (int i = 80; i >= 54; i--)
			{
				swap(ref pattern[i], ref pattern[54 + (int)(27 * rnd())]);
			}
		}

		/// <summary>
		/// To re-create the pattern.
		/// </summary>
		/// <param name="pattern">The pattern array.</param>
		private static void RecreatePattern(int[] pattern)
		{
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void swap<T>(ref T left, ref T right) { var temp = left; left = right; right = temp; }
			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static double rnd() => Rng.NextDouble();

			for (int i = 23; i >= 0; i--)
			{
				swap(ref pattern[i], ref pattern[(int)((i + 1) * rnd())]);
			}
			for (int i = 47; i >= 24; i--)
			{
				swap(ref pattern[i], ref pattern[24 + (int)((i - 23) * rnd())]);
			}
			for (int i = 53; i >= 48; i--)
			{
				swap(ref pattern[i], ref pattern[48 + (int)((i - 47) * rnd())]);
			}
			for (int i = 80; i >= 54; i--)
			{
				swap(ref pattern[i], ref pattern[54 + (int)(27 * rnd())]);
			}
		}
	}
}
