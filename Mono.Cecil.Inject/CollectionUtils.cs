using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Miscellaneous methods for processing enumerable collections.
    /// </summary>
    public static class CollectionUtils
    {
        /// <summary>
        ///     Runs the specified function for each element.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the enumerable.</typeparam>
        /// <param name="self">Reference to the sequence that contains the elements.</param>
        /// <param name="action">Action to apply to each element.</param>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (T var in self)
            {
                action(var);
            }
        }

        /// <summary>
        ///     Gets a range of elements.
        /// </summary>
        /// <param name="self">Refrence to the sequence which to get the values from.</param>
        /// <param name="start">The index of the first element to pick.</param>
        /// <param name="end">The end of the last element to pick.</param>
        /// <typeparam name="T">Type of the sequence.</typeparam>
        /// <returns>A new instance of <see cref="IEnumerable{T}" /> that containes the selected elements.</returns>
        public static IEnumerable<T> Range<T>(this IEnumerable<T> self, int start, int end)
        {
            return self.Where((e, i) => start <= i && i <= end);
        }

        /// <summary>
        ///     Gets a range of elements starting from a specified element.
        /// </summary>
        /// <typeparam name="T">Type of the sequence.</typeparam>
        /// <param name="self">Refrence to the sequence which to get the values from.</param>
        /// <param name="start">The index of the first element to pick.</param>
        /// <param name="count">The total number of elements to select.</param>
        /// <returns>A new instance of <see cref="IEnumerable{T}" /> that containes the selected elements.</returns>
        public static IEnumerable<T> Slice<T>(this IEnumerable<T> self, int start, int count)
        {
            return self.Where((e, i) => start <= i && i < start + count);
        }
    }
}