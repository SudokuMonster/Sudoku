﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoku.Solving.Manual
{
	/// <summary>
	/// Encapsulates a technique code filter that contains some of technique codes.
	/// </summary>
	public sealed class TechniqueCodeFilter : ICloneable<TechniqueCodeFilter>, IEnumerable<TechniqueCode>
	{
		/// <summary>
		/// The internal list.
		/// </summary>
		private readonly BitArray _internalList = new BitArray(typeof(TechniqueCode).GetFields().Length);


		/// <summary>
		/// Initializes an instance with the specified technique codes.
		/// </summary>
		/// <param name="techniqueCodes">(<see langword="params"/> parameter) The technique codes.</param>
		public TechniqueCodeFilter(params TechniqueCode[] techniqueCodes)
		{
			foreach (var techniqueCode in techniqueCodes)
			{
				_internalList[(int)techniqueCode] = true;
			}
		}

		/// <summary>
		/// Initializes an instance with the specified bit array.
		/// </summary>
		/// <param name="bitArray">The bit array.</param>
		private TechniqueCodeFilter(BitArray bitArray) => _internalList = bitArray;


		/// <summary>
		/// To add a technique code.
		/// </summary>
		/// <param name="techniqueCode">The technique code.</param>
		public void Add(TechniqueCode techniqueCode) => _internalList[(int)techniqueCode] = true;

		/// <summary>
		/// Add a serial of technique codes to this list.
		/// </summary>
		/// <param name="techniqueCodes">The codes.</param>
		public void AddRange(IEnumerable<TechniqueCode> techniqueCodes)
		{
			foreach (var techniqueCode in techniqueCodes)
			{
				Add(techniqueCode);
			}
		}

		/// <summary>
		/// To remove a technique code.
		/// </summary>
		/// <param name="techniqueCode">The technique code.</param>
		public void Remove(TechniqueCode techniqueCode) => _internalList[(int)techniqueCode] = false;

		/// <summary>
		/// To determine whether the specified filter contains the technique.
		/// </summary>
		/// <param name="techniqueCode">The technique code to check.</param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		public bool Contains(TechniqueCode techniqueCode) => _internalList[(int)techniqueCode];

		/// <inheritdoc/>
		public IEnumerator<TechniqueCode> GetEnumerator()
		{
			for (int i = 0; i < _internalList.Count; i++)
			{
				if (_internalList[i])
				{
					yield return (TechniqueCode)i;
				}
			}
		}

		/// <inheritdoc/>
		public TechniqueCodeFilter Clone() => new TechniqueCodeFilter((BitArray)_internalList.Clone());

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
