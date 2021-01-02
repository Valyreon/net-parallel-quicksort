using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Valyreon.ParallelQuicksort
{
    internal class SorterDescending<T> : ISorter where T : IComparable, IComparable<T>
    {
        private readonly object key = new object();

        /// <summary>
        /// List of tasks that are currently running.
        /// </summary>
        private readonly IList<Task> runningTasks;

        /// <summary>
        /// Max number of running tasks.
        /// </summary>
        private readonly int maxDegreeOfParallelism;

        /// <summary>
        /// Array of keys which are being used for comparison, the elements in this array are not moved.
        /// </summary>
        private readonly T[] array;

        /// <summary>
        /// Instead of sorting the array, we sort the map which is the array of original indexes.
        /// This is done to avoid false sharing.
        /// </summary>
        private readonly int[] map;

        public event Action Completed;

        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Initializes a descending sorter.
        /// </summary>
        /// <param name="newArray">Array of values that are sorted.</param>
        /// <param name="maxDegreeOfParallelism">Max concurrency level.</param>
        public SorterDescending(T[] newArray, int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }

            runningTasks = new List<Task>();
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;

            array = newArray;

            map = new int[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                map[i] = i;
            }
        }

        public void Start()
        {
            StartAnotherTask(0, array.Length - 1);
        }

        public IEnumerable<int> GetMap()
        {
            return map;
        }

        private void QuickSort(int left, int right)
        {
            do
            {
                // i index counter starts on leftmost index of current iteration
                var i = left;
                // j index counter starts on rightmost index of current iteration
                var j = right;
                // take pivot index as halfway between them
                var x = map[i + ((j - i) >> 1)];
                do
                {
                    // increment index i while members on its position should go before current pivot x
                    while (i < map.Length && array[x].CompareTo(array[map[i]]) < 0)
                    {
                        i++;
                    }

                    // decrement index j while members on its position should go after current pivot x
                    while (j >= 0 && array[x].CompareTo(array[map[j]]) > 0)
                    {
                        j--;
                    }

                    // if i>j that means pivot is in its correct position
                    if (i > j)
                    {
                        break;
                    }

                    // if i < j that means we found an element on the left side that should go on the right side of the pivot
                    // and we found an element on the right side that should go on the left side of the pivot so we swap them
                    if (i < j)
                    {
                        var temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }

                    // increment i and j index counters because we know that elements they are pointing at are on the correct side
                    i++;
                    j--;
                } while (i <= j);
                // here we determine which of the two parts we quicksort fist, because we want to quicksort the shorter one first
                // j-left is number of elements on the left side of the pivot
                // right-i is number of elements on the right side of the pivot
                if (j - left <= right - i)
                {
                    // we sort the left side of the pivot
                    // sort only if there is more than one element
                    if (left < j)
                    {
                        // if number of elements is less or equal to 16 do insertion sort in place
                        // insertion sort is faster than quicksort for small arrays
                        if (j - left <= 16)
                        {
                            InsertionSort(left, j);
                        }
                        else
                        {
                            // if number of elements > 16 first try to start another thread that will sort this part
                            var deferred = false;
                            lock (key)
                            {
                                // if we have space for a new task
                                if (maxDegreeOfParallelism > runningTasks.Count)
                                {
                                    StartAnotherTask(left, j);
                                    deferred = true;
                                }
                            }

                            // if we couldnt start a new thread because max parallelism is reached do the part here
                            if (!deferred)
                            {
                                QuickSort(left, j);
                            }
                        }
                    }

                    // we move the left side to i so in the next iteration the right side will be sorted
                    left = i;
                }
                else
                {
                    // we sort the right side of the pivot
                    // sort only if there is more than one element
                    if (i < right)
                    {
                        // if number of elements is less or equal to 16 do insertion sort in place
                        // insertion sort is faster than quicksort for small arrays
                        if (right - i <= 16)
                        {
                            InsertionSort(i, right);
                        }
                        else
                        {
                            QuickSort(i, right);
                        }
                    }

                    // we move the right side to j so in the next iteration the left side will be sorted
                    right = j;
                }
            } while (left < right);
        }

        public void InsertionSort(int startIndex, int endIndex)
        {
            for (var i = startIndex + 1; i <= endIndex; ++i)
            {
                var key = array[map[i]];
                var index = map[i];
                var j = i - 1;
                while (j >= startIndex && array[map[j]].CompareTo(key) < 0)
                {
                    map[j + 1] = map[j];
                    --j;
                }
                map[j + 1] = index;
            }
        }

        private void StartAnotherTask(int left, int right)
        {
            Task taskLower = null;
            taskLower = new Task(() =>
            {
                QuickSort(left, right);
                lock (key)
                {
                    _ = runningTasks.Remove(taskLower);
                    if (runningTasks.Count == 0)
                    {
                        Completed?.Invoke();
                    }
                }
            });

            runningTasks.Add(taskLower);
            taskLower.Start();
        }
    }
}
