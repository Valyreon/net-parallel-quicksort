using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Valyreon.ParallelQuicksort.Tests
{
	public static class MyAssert
	{
		public static void IsSorted(IEnumerable<int> arr)
		{
			Assert.IsTrue(IsSortedHelper(arr), "Array is not sorted.");
		}

		public static void IsSortedDescending(IEnumerable<int> arr)
		{
			Assert.IsTrue(IsSortedHelper(arr, true), "Array is not sorted.");
		}

		private static bool IsSortedHelper(IEnumerable<int> arr, bool descending = false)
		{
			Func<int, int, bool> comparer = descending ? (x, y) => x < y : (x, y) => x > y;

			var isFirst = true;
			var last = 0;
			foreach(var x in arr)
			{
				if(isFirst)
				{
					isFirst = false;
					last = x;
					continue;
				}
				else if(comparer(last, x))
				{
					return false;
				}

				last = x;
			}

			return true;
		}
	}
}
