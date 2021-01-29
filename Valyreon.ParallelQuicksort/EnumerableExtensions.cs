using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Valyreon.ParallelQuicksort
{
    public static class EnumerableExtensions
    {
        private const int ParallelThreshold = 8000;

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="concurrencyLevel">Maximum level of parallelisation.</param>
        /// <returns>An System.Linq.IEnumerable whose elements are sorted according to a key.</returns>
        public static IEnumerable<TSource> ParallelOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4)
            where TKey : IComparable, IComparable<TKey>
        {
            return ParallelOrderByInternal(source, keySelector, concurrencyLevel);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key async.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="concurrencyLevel">Maximum level of parallelisation.</param>
        /// <returns>An System.Linq.IEnumerable whose elements are sorted according to a key.</returns>
        public static Task<IEnumerable<TSource>> ParallelOrderByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4)
            where TKey : IComparable, IComparable<TKey>
        {
            return ParallelOrderByInternalAsync(source, keySelector, concurrencyLevel);
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="concurrencyLevel">Maximum level of parallelisation.</param>
        /// <returns>An System.Linq.IEnumerable whose elements are sorted according to a key.</returns>
        public static IEnumerable<TSource> ParallelOrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4)
            where TKey : IComparable, IComparable<TKey>
        {
            return ParallelOrderByInternal(source, keySelector, concurrencyLevel, true);
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key async.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="concurrencyLevel">Maximum level of parallelisation.</param>
        /// <returns>An System.Linq.IEnumerable whose elements are sorted according to a key.</returns>
        public static Task<IEnumerable<TSource>> ParallelOrderByDescendingAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4)
            where TKey : IComparable, IComparable<TKey>
        {
            return ParallelOrderByInternalAsync(source, keySelector, concurrencyLevel, true);
        }

        private static IEnumerable<TSource> ParallelOrderByInternal<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4, bool descending = false)
            where TKey : IComparable, IComparable<TKey>
        {
            var enumerable = source as TSource[] ?? source.ToArray();
            if (enumerable.Count() <= ParallelThreshold)
            {
                return descending ? enumerable.OrderByDescending(keySelector) : enumerable.OrderBy(keySelector);
            }

            var sorter = descending
                ? (ISorter)new SorterDescending<TKey>(enumerable.Select(keySelector).ToArray(), concurrencyLevel)
                : new Sorter<TKey>(enumerable.Select(keySelector).ToArray(), concurrencyLevel);
            var cancelSource = new CancellationTokenSource();

            sorter.Completed += () => cancelSource.Cancel();

            sorter.Start();
            var waitTask = Task.Run(async () =>
            {
                while (!sorter.IsCompleted)
                {
                    try
                    {
                        await Task.Delay(5000, cancelSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }, cancelSource.Token);

            Task.WaitAll(waitTask);

            var arrayToSort = enumerable.ToArray();
            return sorter.GetMap().Select(x => arrayToSort[x]);
        }

        private static async Task<IEnumerable<TSource>> ParallelOrderByInternalAsync<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int concurrencyLevel = 4, bool descending = false)
            where TKey : IComparable, IComparable<TKey>
        {
            var enumerable = source as TSource[] ?? source.ToArray();
            if (enumerable.Count() <= ParallelThreshold)
            {
                return descending ? enumerable.OrderByDescending(keySelector) : enumerable.OrderBy(keySelector);
            }

            var sorter = descending
                ? (ISorter)new SorterDescending<TKey>(enumerable.Select(keySelector).ToArray(), concurrencyLevel)
                : new Sorter<TKey>(enumerable.Select(keySelector).ToArray(), concurrencyLevel);
            var cancelSource = new CancellationTokenSource();

            sorter.Completed += () => cancelSource.Cancel();

            sorter.Start();
            await Task.Run(async () =>
            {
                while (!sorter.IsCompleted)
                {
                    try
                    {
                        await Task.Delay(5000, cancelSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }, cancelSource.Token);

            var arrayToSort = enumerable.ToArray();
            return sorter.GetMap().Select(x => arrayToSort[x]);
        }
    }
}
