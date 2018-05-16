using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	public class PRGFactory
	{
		/// <summary>
		/// Returns an initialized instance of a PRG
		/// </summary>
		public static IPRG GetPRG(Nullable<int> seed = null)
		{
			return new DefaultRandom(seed.HasValue ? seed.Value : new Random().Next());
		}
	}

	public interface IPRG
	{
		int Next();
		int Next(int max);
		int Next(int min, int max);
		void NextBytes(byte[] bytes);
		IPRG RecreateWithSeed(int seed);
	}

	public class DefaultRandom : IPRG
	{
		private Random _generator;

		public DefaultRandom(int seed)
		{
			_generator = new Random(seed);
		}

		public int Next()
		{
			return _generator.Next();
		}

		public int Next(int max)
		{
			return _generator.Next(max);
		}

		public int Next(int min, int max)
		{
			return _generator.Next(min, max);
		}

		public void NextBytes(byte[] bytes)
		{
			_generator.NextBytes(bytes);
		}

		public IPRG RecreateWithSeed(int seed)
		{
			return new DefaultRandom(seed);
		}
	}
}
