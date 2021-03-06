using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Valyreon.ParallelQuicksort.Tests
{
    [TestClass]
    public class AveragesTest
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// First sort is always slower, so to not affect results use this before.
        /// </summary>
        private static void Warmup()
        {
            var array = Utilities.GenerateRandomArray(1000000);
            _ = array.ParallelOrderBy(x => x).ToArray();
            _ = array.OrderBy(x => x).ToArray();
        }

        private static int MyRound(double d)
        {
            return (int)(d + 0.5);
        }

        [TestMethod]
        public void TestAveragesLinqVsParallel()
        {
            // takes 82min for full array
            var sizes = new int[] { 10000, 50000, 250000, 1000000, 5000000, 25000000 };
            var sizesFriendly = new string[] { "10k", "50k", "250k", "1m", "5m", "25m" };

            Assert.AreEqual(sizes.Length, sizesFriendly.Length);
            var lines = new List<string>() { "#,PARALLEL,LINQ" };

            Warmup();
            for (var i = 0; i < sizes.Length; ++i)
            {
                var paralellTimes = new List<long>();
                var linqTimes = new List<long>();

                // takes ~20s for 1
                for (var j = 0; j < 10; ++j)
                {
                    var array = Utilities.GenerateRandomArray(sizes[i]);

                    var stopwatch = Stopwatch.StartNew();
                    _ = array.ParallelOrderBy(x => x).ToArray();
                    stopwatch.Stop();
                    paralellTimes.Add(stopwatch.ElapsedMilliseconds);

                    stopwatch = Stopwatch.StartNew();
                    _ = array.OrderBy(x => x).ToArray();
                    stopwatch.Stop();
                    linqTimes.Add(stopwatch.ElapsedMilliseconds);
                }

                lines.Add($"{sizesFriendly[i]},{MyRound(paralellTimes.Average())},{MyRound(linqTimes.Average())}");
            }

            File.WriteAllLines("averages.csv", lines);
        }

        [TestMethod]
        public void FindNForWhichParallelIsFaster()
        {
            // remember to comment out code which goes to linq below threshold in EnumerableExtensions
            var results = new List<double>();
            Warmup();
            // takes ~4s for 100
            // last result 7519 over 100k iterations
            for (var i = 0; i < 5; ++i)
            {
                var size = 5000;
                var found = false;
                while (!found)
                {
                    var array = Utilities.GenerateRandomArray(size);

                    var stopwatch1 = Stopwatch.StartNew();
                    var result1 = array.ParallelOrderBy(x => x).ToArray();
                    stopwatch1.Stop();
                    //MyAssert.IsSorted(result1);

                    var stopwatch2 = Stopwatch.StartNew();
                    var result2 = array.OrderBy(x => x).ToArray();
                    stopwatch2.Stop();
                    //MyAssert.IsSorted(result2);

                    Assert.IsTrue(result1.SequenceEqual(result2), "Both results should be in the same order.");

                    if (stopwatch2.ElapsedMilliseconds > stopwatch1.ElapsedMilliseconds)
                    {
                        results.Add(size);
                        found = true;
                    }

                    size += 100;
                }
            }

            TestContext.WriteLine($"N: {results.Average()}");
        }
    }
}
