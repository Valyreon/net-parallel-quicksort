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
            MyAssert.IsSorted(sortedArray);
        }

        [TestMethod]
        public void TestSortDescending()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            var sortedArray = array.ParallelOrderByDescending(x => x).ToArray();
            MyAssert.IsSortedDescending(sortedArray);
        }

        [TestMethod]
        public Task TestSortAscendingAsync()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            return Task.Run(async () =>
            {
                var sortedArray = (await array.ParallelOrderByAsync(x => x)).ToArray();
                MyAssert.IsSorted(sortedArray);
            });
        }

        [TestMethod]
        public Task TestSortDescendingAsync()
        {
            var array = Utilities.GenerateRandomArray(ArraySize);
            return Task.Run(async () =>
            {
                var sortedArray = await array.ParallelOrderByDescendingAsync(x => x);
                MyAssert.IsSortedDescending(sortedArray);
            });
        }

        [TestMethod]
        public void TestSortEmptyAscending()
        {
            var array = System.Array.Empty<int>();
            var sortedArray = array.ParallelOrderBy(x => x);
            Assert.IsTrue(!sortedArray.Any());
            MyAssert.IsSorted(sortedArray);
        }

        [TestMethod]
        public void TestSortEmptyDescending()
        {
            var array = System.Array.Empty<int>();
            var sortedArray = array.ParallelOrderByDescending(x => x);
            Assert.IsTrue(!sortedArray.Any());
            MyAssert.IsSortedDescending(sortedArray);
        }
    }
}
