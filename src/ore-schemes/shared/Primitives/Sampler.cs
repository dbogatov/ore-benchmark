using System;
using Accord.Statistics.Distributions.Univariate;

namespace ORESchemes.Shared.Primitives
{
	public class SamplerFactory
	{
		/// <summary>
		/// Returns an initialized instance of a ISampler
		/// </summary>
		public static ISampler GetSampler(Nullable<int> entropy = null)
		{
			return new AccordSampler(entropy.HasValue ? entropy.Value : new Random().Next());
		}
	}

	public interface ISampler
	{
		int HyperGeometric(int population, int successes, int samples);
		int Uniform(int from, int to);
	}

	public class AccordSampler : ISampler
	{
		private Random _generator;

		public AccordSampler(int entropy)
		{
			_generator = new Random(entropy);
		}

		public int HyperGeometric(int population, int successes, int samples)
		{
			return new HypergeometricDistribution(population, successes, samples).Generate(_generator);
		}

		public int Uniform(int from, int to)
		{
			return new UniformDiscreteDistribution(from, to).Generate(_generator);
		}
	}
}
