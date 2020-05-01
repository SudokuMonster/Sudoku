﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sudoku.Extensions;

namespace Sudoku.Data.Collections
{
	/// <summary>
	/// Indicates a region collection.
	/// </summary>
	public readonly ref struct RegionCollection
	{
		/// <summary>
		/// The inner mask.
		/// </summary>
		private readonly int _mask;


		/// <summary>
		/// Initializes an instance with the specified regions.
		/// </summary>
		/// <param name="regions">The regions.</param>
		public RegionCollection(ReadOnlySpan<int> regions)
		{
			_mask = 0;
			foreach (int region in regions)
			{
				_mask |= 1 << region;
			}
		}

		/// <summary>
		/// Initializes an instance with the specified regions.
		/// </summary>
		/// <param name="regions">The regions.</param>
		public RegionCollection(IEnumerable<int> regions)
		{
			_mask = 0;
			foreach (int region in regions)
			{
				_mask |= 1 << region;
			}
		}


		/// <summary>
		/// Indicates the number of regions that contain in this collection.
		/// </summary>
		public int Count => _mask.CountSet();


		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">Always throws.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DoesNotReturn]
		public override bool Equals(object? obj) => throw Throwing.RefStructNotSupported;

		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="Equals" and @paramType="__any"]'/>
		public bool Equals(RegionCollection other) => _mask == other._mask;

		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="GetHashCode"]'/>
		/// <exception cref="NotSupportedException">Always throws.</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DoesNotReturn]
		public override int GetHashCode() => throw Throwing.RefStructNotSupported;

		/// <include file='../GlobalDocComments.xml' path='comments/method[@name="ToString" and @paramType="__noparam"]'/>
		public override string ToString()
		{
			if (Count == 0)
			{
				return string.Empty;
			}

			if (Count == 1)
			{
				int region = _mask.FindFirstSet();
				return $"{GetLabel(region / 9)}{region % 9 + 1}";
			}

			var dic = new Dictionary<int, ICollection<int>>();
			foreach (int region in this)
			{
				if (!dic.ContainsKey(region / 9))
				{
					dic.Add(region / 9, new List<int>());
				}

				dic[region / 9].Add(region % 9);
			}

			var sb = new StringBuilder();
			for (int i = 0; i < 3; i++)
			{
				if (!dic.ContainsKey(i))
				{
					continue;
				}

				sb.Append(GetLabel(i));
				foreach (int z in dic[i])
				{
					sb.Append(z + 1);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// To string but only output the labels ('r', 'c' or 'b').
		/// </summary>
		/// <returns>The labels.</returns>
		public string ToSimpleString()
		{
			var sb = new StringBuilder();
			foreach (int region in this)
			{
				sb.Append(GetLabel(region / 9));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Get the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<int> GetEnumerator() => _mask.GetAllSets().GetEnumerator();

		/// <summary>
		/// Get the label of each region.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns>The label.</returns>
		private char GetLabel(int index)
		{
			return index switch
			{
				0 => 'b',
				1 => 'r',
				2 => 'c',
				_ => throw Throwing.ImpossibleCase
			};
		}


		/// <include file='../GlobalDocComments.xml' path='comments/operator[@name="op_Equality"]'/>
		public static bool operator ==(RegionCollection left, RegionCollection right) => left.Equals(right);

		/// <include file='../GlobalDocComments.xml' path='comments/operator[@name="op_Inequality"]'/>
		public static bool operator !=(RegionCollection left, RegionCollection right) => !(left == right);
	}
}
