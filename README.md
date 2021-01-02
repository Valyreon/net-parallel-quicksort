# .NET Parallel Quicksort Library
![](https://img.shields.io/badge/NuGet-v1.0.0-blueviolet.svg)

This is a library for parallel quicksort I created after running into the problem of false sharing when working on some project. First I solved the problem by padding the variables to fill the cache line but I still couldn't get quite the best speed. In the end, the solution was simple, instead of sorting the array, sort a map which contains element indexes and then just return the values in that order. Then I wanted to see what kind of performance I could get out of a parallel quicksort algorithm so I created this .NET Standard library and tinkered the standard algorithm to get the best numbers.

## Usage

You can install the **[NuGet package](https://www.nuget.org/packages/Valyreon.ParallelQuicksort/1.0.0)** to use the library or manually compile it yourself, copy the code or whatever.

The sort is an extension method for IEnumerable and it's called exactly like LINQ's OrderBy and has async version as well. Example:
```csharp
var sortedArrayAscending = list.ParallelOrderBy(x => x).ToArray();
var sortedListDescending = await list.ParallelOrderByDescendingAsync(x => x, 3).ToList();
```
You can also specify the max degree of parallelism that the algorithm will try to use as the second parameter. Default value is 4. It uses the Task class to achieve concurrency so of course parallelism isn't guaranteed because the scheduling itself is left to the OS.

## Algorithm

Quicksort algorithm here chooses the middle most element as the pivot and then 'rotates' other elements around it to get them on the right side. Once done with pivot it will check if max concurrency is reached. If not, it will create a new Task to sort one side and it will continue sorting the other side on the next iteration. Also if the part that needs sorting has <= 16 elements, it will be sorted using insertion algorithm without creating a new task.

I played around with Insertion algorithm and tested times, and 16 seemed like a good threshold. At first I tested times when I called Insertion sort in the new Task but that caused greater sorting times with overhead of creating new Tasks and scheduling them (overthreading basically).

Also, due to overhead of creating new Tasks and overthreading there is a threshold in array size before which serial quicksort is better than parallel. After testing over 100 000 iterations against LINQ's OrderBy on randomly generated arrays, the average point in size was 7519. So i decided that for collections that have 8000 members or less, the algorithm will just call LINQ's OrderBy.

## Performance analysis

I tested the library and got average sorting times for arrays of various sizes with 250 calls for each array size. The max concurrency was the default 4. You can find the test in the AveragesTest in the Tests project. Here are the generated graphs from the results:

![Graph 1](https://raw.githubusercontent.com/Valyreon/net-parallel-quicksort/main/graph1.png)

![Graph 2](https://raw.githubusercontent.com/Valyreon/net-parallel-quicksort/main/graph2.png)

From the graphs we can see that the performance gain is significant for larger arrays, on arrays with length 250k the parallel algorithm is on average 3 times as fast as the serial version with the difference increasing.
---

