using System;
using System.Linq;

namespace Valyreon.ParallelQuicksort.Tests
{
    public static class Utilities
    {
        public static int[] GenerateRandomArray(int size)
        {
            var array = new int[size];
            var random = new Random();

            for (var i = 0; i < size; i++)
            {
                array[i] = random.Next(1, size * 2);
            }

            return array;
        }

        public static string GenerateRandomString(int length)
        {
            var random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int GenerateRandomNumber(int max)
        {
            var random = new Random();
            return random.Next(max);
        }
    }
}
