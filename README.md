# .NET Parallel Quicksort Library

This is a library for parallel quicksort I created after doing a project for a course at University in which I ran into the problem of false sharing. First I solved the problem by padding the variables but I still couldn't get quite the speed of LINQ's OrderBy so I went through the .NET Core source code and found the quicksort algorithm they were using. The solution was simple, instead of sorting the array, sort a map which contains element indexes and then just return the values in that order. Then I wanted to see what kind of performance I could get out of a parallel quicksort algorithm so I created this .NET Standard library and experimented to get the best numbers.

## Usage

To use you can install the nuget package.

The sort is an extension method for IEnumerable and it's called exactly like LINQ's OrderBy and has async version as well. Example:
```csharp
var array = Utilities.GenerateRandomArray(ArraySize);

var sortedArrayAscending = array.ParallelOrderBy(x => x).ToArray();
var sortedArrayDescending = await array.ParallelOrderByDescendingAsync(x => x, 3).ToArray();
```
You can also specify the max degree of parallelism that the algorithm will try to use as the second parameter. Default value is 4. It uses the .NET Task class to achieve concurrency so of course parallelism isn't guaranteed because the scheduling itself is left to the OS.

## Algorithm

Quicksort algorithm here chooses the middle most element as the pivot and then 'rotates' other elements around it to get them on the right side. Once done it will check if max concurrency is reached. If not, it will create a new Task to sort one side and it will continue sorting the other side on the next iteration. Also if the part that needs sorting has <= 16 elements, it will be sorted using insertion algorithm without creating a new task. 

I played around with Insertion algorithm and tested times, and 16 seemed like a good threshold. At first I tested times when I called Insertion sort in the new Task but that caused greater sorting times with overhead of creating new Tasks and scheduling them (overthreading basically).

Also, due to overhead of creating new Tasks and overthreading there is a threshold in array size before which serial quicksort is better than parallel. After testing over 100 000 iterations against LINQ's OrderBy, the average point in size was 7519. So i decided that for collections that have 8000 members or less, the algorithm will just revert back to LINQ's OrderBy.

## Performance analysis

I tested the library and got average sorting times for arrays of various sizes with 250 calls for each array size. You can find the test in the AveragesTest in the Tests project. Here are the generated graphs from the results:

<img src="https://raw.githubusercontent.com/Valyreon/net-parallel-quicksort/main/graph1.png" width="800">

<img src="https://raw.githubusercontent.com/Valyreon/net-parallel-quicksort/main/graph2.png" width="800">

