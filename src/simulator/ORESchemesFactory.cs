using System;
using ORESchemes.Shared;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;
using ORESchemes.LewiORE;

namespace Simulation
{
	/// <summary>
	/// Generic class for ORE factories
	/// </summary>
	/// <typeparam name="S">Scheme type</typeparam>
	/// <typeparam name="C">Scheme's ciphertext type</typeparam>
	public abstract class ORESchemesFactory<S, C> where S : IOREScheme<C>
	{
		protected readonly byte[] _entropy;

		public ORESchemesFactory(int? seed = null)
		{
			if (seed != null)
			{
				_entropy = BitConverter.GetBytes(seed.Value);
			}
			else
			{
				_entropy = new byte[256 / 8];
				new Random().NextBytes(_entropy);
			}
		}

		/// <summary>
		/// Returns an initialized scheme with default / suggested parameters
		/// </summary>
		public abstract S GetScheme();
	}

	public class NoEncryptionFactory : ORESchemesFactory<NoEncryptionScheme, long>
	{
		public NoEncryptionFactory(int? seed = null) : base(seed) { }

		public override NoEncryptionScheme GetScheme() => new NoEncryptionScheme(_entropy);
	}

	public class PracticalOREFactory : ORESchemesFactory<PracticalOREScheme, ORESchemes.PracticalORE.Ciphertext>
	{
		public PracticalOREFactory(int? seed = null) : base(seed) { }

		public override PracticalOREScheme GetScheme() => new PracticalOREScheme(_entropy);
	}

	public class CryptDBOPEFactory : ORESchemesFactory<CryptDBScheme, long>
	{
		public CryptDBOPEFactory(int? seed = null) : base(seed) { }

		public override CryptDBScheme GetScheme() =>
			new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(Int32.MinValue) * 100000, // reasonable maximum
				Convert.ToInt64(Int32.MaxValue) * 100000, // larger numbers cause performance degradation
				_entropy
			);
	}

	public class LewiOREFactory : ORESchemesFactory<LewiOREScheme, ORESchemes.LewiORE.Ciphertext>
	{
		public LewiOREFactory(int? seed = null) : base(seed) { }

		public override LewiOREScheme GetScheme() => new LewiOREScheme(16, _entropy);
	}
}

