using System;
using Accord.Statistics.Distributions.Univariate;

namespace ORESchemes.Shared.Primitives
{
	public class SamplerFactory
	{
		/// <summary>
		/// Returns an initialized instance of a ISampler that works on 64 bits integers
		/// </summary>
		public static ISampler<long> GetSampler(byte[] entropy = null)
		{
			return new CustomSampler(entropy);
		}

		// /// <summary>
		// /// Returns an initialized instance of a ISampler that works on 64 bits integers
		// /// </summary>
		// public static ISampler<long> GetSampler(Nullable<long> entropy = null)
		// {
		// 	return new CustomSampler(entropy.HasValue ? entropy.Value : new Random().Next());
		// }
	}

	public interface ISampler<T> where T : struct
	{
		T HyperGeometric(T population, T successes, T samples);
		T Uniform(T from, T to);
	}

	// public class AccordSampler : ISampler<int>
	// {
	// 	private Random _generator;

	// 	public AccordSampler(int entropy)
	// 	{
	// 		_generator = new Random(entropy);
	// 	}

	// 	public int HyperGeometric(int population, int successes, int samples)
	// 	{
	// 		return new HypergeometricDistribution(population, successes, samples).Generate(_generator);
	// 	}

	// 	public int Uniform(int from, int to)
	// 	{
	// 		return new UniformDiscreteDistribution(from, to).Generate(_generator);
	// 	}
	// }

	public class CustomSampler : ISampler<long>
	{
		private IPRG _generator;

		public CustomSampler(byte[] entropy = null)
		{
			_generator = PRGFactory.GetPRG(entropy);
		}

		// https://github.com/mathnet/mathnet-numerics
		public long HyperGeometric(long population, long successes, long samples)
		{
			var x = 0;

			do
			{
				var p = (double)successes / population;
				var r = _generator.NextDouble();
				if (r < p)
				{
					x++;
					successes--;
				}

				population--;
				samples--;
			}
			while (0 < samples);

			return x;
		}

		// https://github.com/mathnet/mathnet-numerics
		public long Uniform(long from, long to)
		{
			return from + _generator.Next() * (to - from);
		}
	}
}
