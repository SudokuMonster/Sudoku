﻿namespace Sudoku.Solving.Manual
{
	partial class ManualSolver
	{
		/// <summary>
		/// <para>Indicates the solver will optimizes the applying order.</para>
		/// <para>
		/// When the value is <c>true</c>, the result to apply to
		/// the grid will be the one which has the minimum difficulty
		/// rating; otherwise, the applying step will be the first one
		/// of all steps found.
		/// </para>
		/// </summary>
		public bool OptimizedApplyingOrder { get; set; } = false;

		/// <summary>
		/// Indicates whether the solver will record the step
		/// whose name or kind is full house.
		/// </summary>
		/// <remarks>
		/// Full houses are the techniques that used in a single
		/// region. When the specified region has only one empty cell,
		/// the full house will be found at this empty cell (the last
		/// value in this region).
		/// </remarks>
		public bool EnableFullHouse { get; set; } = true;

		/// <summary>
		/// Indicates whether the solver will record the step
		/// whose name or kind is last digit.
		/// </summary>
		/// <remarks>
		/// Last digits are the techniques that used in a single
		/// digit. When the whole grid has 8 same digits, the last
		/// one will be always found and set in the last position,
		/// which is last digit.
		/// </remarks>
		public bool EnableLastDigit { get; set; } = true;

		/// <summary>
		/// Indicates whether the solver will be forced to find all
		/// assigment conclusions with value 1 to 9 in order.
		/// </summary>
		public bool IttoRyuWhenPossible { get; set; } = false;


		/// <summary>
		/// The field bound with <see cref="CheckRegularWingSize"/>.
		/// </summary>
		/// <seealso cref="CheckRegularWingSize"/>
		private int _checkRegularWingSize;

		/// <summary>
		/// Indicates all regular wings with the size less than
		/// or equals to this specified value.
		/// </summary>
		/// <remarks>
		/// In fact this value can be 9 at most (i.e. <c>value &gt;&#61; 3
		/// &amp;&amp; value &lt;&#61; 9</c>), however the searching
		/// is too low so I do not allow them.
		/// </remarks>
		public int CheckRegularWingSize
		{
			get => _checkRegularWingSize;
			set => _checkRegularWingSize = value >= 3 && value <= 5 ? value : 5;
		}
	}
}
