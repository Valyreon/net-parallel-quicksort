using RandN;
using RandN.Distributions;

namespace Valyreon.ParallelQuicksort.Tests
{
	public static class Utilities
	{
		public static int[] GenerateRandomArray(int size)
		{
			var array = new int[size];

			var rng = StandardRng.Create();
			var d6 = Uniform.NewInclusive(1, size * 2);

			for(var i = 0 ; i < size ; i++)
			{
				array[i] = d6.Sample(rng);
			}

			return array;
		}
	}
}
