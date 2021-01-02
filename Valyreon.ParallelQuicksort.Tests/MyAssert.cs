using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Valyreon.ParallelQuicksort.Tests
{
    public static class MyAssert
    {
        public static void IsSorted<TSource, TKey>(IEnumerable<TSource> arr, Func<TSource, TKey> keySelector)
            where TKey : IComparable, IComparable<TKey>
        {
            Assert.IsTrue(IsSortedHelper(arr, keySelector), "Array is not sorted.");
        }

        public static void IsSortedDescending<TSource, TKey>(IEnumerable<TSource> arr, Func<TSource, TKey> keySelector)
            where TKey : IComparable, IComparable<TKey>
        {
            Assert.IsTrue(IsSortedHelper(arr, keySelector, true), "Array is not sorted.");
        }

        private static bool IsSortedHelper<TSource, TKey>(IEnumerable<TSource> arr, Func<TSource, TKey> keySelector, bool descending = false)
            where TKey : IComparable, IComparable<TKey>
        {
            Func<TKey, TKey, bool> comparer = descending ? (x, y) => x.CompareTo(y) < 0 : (x, y) => x.CompareTo(y) > 0;

            var isFirst = true;
            TKey last = default;
            foreach (var x in arr.Select(keySelector))
            {
                if (isFirst)
                {
                    isFirst = false;
                    last = x;
                    continue;
                }
                else if (comparer(last, x))
                {
                    return false;
                }

                last = x;
            }

            return true;
        }
    }
}
