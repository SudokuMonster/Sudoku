﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Linq
{
	/// <summary>
	/// Provides a set of static methods for querying objects that implement
	/// <see cref="IEnumerable{T}"/> and <see cref="Collections.IEnumerable"/>.
	/// </summary>
	/// <remarks>
	/// This class has the same function and status with <see cref="Enumerable"/>.
	/// </remarks>
	/// <seealso cref="Collections.IEnumerable"/>
	/// <seealso cref="IEnumerable{T}"/>
	/// <seealso cref="Enumerable"/>
	public static class EnumerableEx
	{
		/// <summary>
		/// Get the element whose selection is the minimal one.
		/// </summary>
		/// <typeparam name="TElement">The element type.</typeparam>
		/// <typeparam name="TComparable">The comparing type.</typeparam>
		/// <param name="elements">The elements to search the minimal one.</param>
		/// <param name="selector">The selector.</param>
		/// <returns>
		/// The result value. If the collection does not have a minimal element,
		/// the result will be the default value, where it will be <c>null</c>, which
		/// is decided in the element type.
		/// </returns>
		[return: MaybeNull]
		public static TElement GetElementByMinSelector<TElement, TComparable>(
			this IEnumerable<TElement> elements, Func<TElement, IComparable<TComparable>> selector)
			where TElement : notnull =>
			(from element in elements orderby selector(element) select element).FirstOrDefault();

		/// <summary>
		/// Get the total number of the elements using the specified formula.
		/// </summary>
		/// <typeparam name="TElement">The type of element.</typeparam>
		/// <param name="elements">All elements.</param>
		/// <param name="countingFormula">The formula used in counting.</param>
		/// <returns>The total number.</returns>
		public static int Count<TElement>(
			this IEnumerable<TElement> elements, Func<TElement, int> countingFormula)
			where TElement : notnull
		{
			int count = 0;
			foreach (var element in elements)
			{
				count += countingFormula(element);
			}

			return count;
		}

		/// <summary>
		/// Get the total number of a part of elements using the specified formula.
		/// </summary>
		/// <typeparam name="TElement">The type of element.</typeparam>
		/// <param name="elements">All elements.</param>
		/// <param name="selector">The selector to get the specified elements.</param>
		/// <param name="countingFormula">The formula used in counting.</param>
		/// <returns>The total number.</returns>
		public static int Count<TElement>(
			this IEnumerable<TElement> elements, Predicate<TElement> selector,
			Func<TElement, int> countingFormula)
			where TElement : notnull
		{
			int count = 0;
			foreach (var element in elements)
			{
				if (selector(element))
				{
					count += countingFormula(element);
				}
			}

			return count;
		}

		/// <summary>
		/// Convert the collection into a read-only list.
		/// </summary>
		/// <typeparam name="TElement">The type of the element.</typeparam>
		/// <param name="elements">All elements.</param>
		/// <returns>The read-only list.</returns>
		public static IReadOnlyList<TElement> ToReadOnlyList<TElement>(
			this IEnumerable<TElement> elements)
			where TElement : notnull
		{
			return elements switch
			{
				IList<TElement> list => (IReadOnlyList<TElement>)list,
				_ => new List<TElement>(elements)
			};
		}
	}
}
