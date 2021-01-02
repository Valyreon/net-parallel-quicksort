using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Valyreon.ParallelQuicksort.Tests
{
    [TestClass]
    public class BasicTests
    {
        private static readonly int ArraySize = 11000;

        [TestMethod]
        public void TestSortAscending()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            var sortedArray = array.ParallelOrderBy(x => x).ToArray();
            MyAssert.IsSorted(sortedArray, x => x);
        }

        [TestMethod]
        public void TestSortDescending()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            var sortedArray = array.ParallelOrderByDescending(x => x).ToArray();
            MyAssert.IsSortedDescending(sortedArray, x => x);
        }

        [TestMethod]
        public void TestSortAscendingAsync()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            Task.Run(async () =>
            {
                var sortedArray = (await array.ParallelOrderByAsync(x => x)).ToArray();
                MyAssert.IsSorted(sortedArray, x => x);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestSortDescendingAsync()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            Task.Run(async () =>
            {
                var sortedArray = await array.ParallelOrderByDescendingAsync(x => x);
                MyAssert.IsSortedDescending(sortedArray, x => x);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestSortEmptyAscending()
        {
            var array = System.Array.Empty<int>();
            var sortedArray = array.ParallelOrderBy(x => x);
            Assert.IsTrue(!sortedArray.Any());
            MyAssert.IsSorted(sortedArray, x => x);
        }

        [TestMethod]
        public void TestSortEmptyDescending()
        {
            var array = System.Array.Empty<int>();
            var sortedArray = array.ParallelOrderByDescending(x => x);
            Assert.IsTrue(!sortedArray.Any());
            MyAssert.IsSortedDescending(sortedArray, x => x);
        }

        [TestMethod]
        public void TestObjectSorting()
        {
            var list = new List<ExampleObject>();

            for (var i = 0; i < ArraySize; i++)
            {
                list.Add(new ExampleObject
                {
                    Name = Utilities.GenerateRandomString(12),
                    Priority = Utilities.GenerateRandomNumber(100)
                });
                ;
            }

            var sortedList = list.ParallelOrderByDescending(x => x.Name).ToList();
            MyAssert.IsSortedDescending(sortedList, x => x.Name);

            var sortedListAscending = list.ParallelOrderBy(x => x.Priority).ToList();
            MyAssert.IsSorted(sortedListAscending, x => x.Priority);
        }
    }
}
