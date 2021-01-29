using System;
using System.Collections.Generic;

namespace Valyreon.ParallelQuicksort
{
    internal interface ISorter
    {
        /// <summary>
        /// Event that fires when the sorter finishes sorting the array.
        /// </summary>
        event Action Completed;

        /// <summary>
        /// Starts the first task that uses quicksort to sort the array. This task will should create other tasks
        /// as necessary.
        /// </summary>
        void Start();

        /// <summary>
        /// Bool that indicates if sorter is finished sorting.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets the resulting map of indexes according to which the array should be sorted.
        /// </summary>
        /// <returns>Array containing sorted order of indexes.</returns>
        IEnumerable<int> GetMap();
    }
}
