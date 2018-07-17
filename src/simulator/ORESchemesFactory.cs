using System;
using ORESchemes.Shared;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;
using ORESchemes.LewiORE;
using ORESchemes.FHOPE;
using ORESchemes.AdamORE;

namespace Simulation
{
	/// <summary>
	/// Generic class for ORE factories
	/// </summary>
	/// <typeparam name="S">Scheme type</typeparam>
	/// <typeparam name="C">Scheme's ciphertext type</typeparam>
	/// <typeparam name="K">Scheme's key type</typeparam>
	public abstract class ORESchemesFactory<S, C, K>
		where S : IOREScheme<C, K>
		where C : IGetSize
		where K : IGetSize
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
				_entropy = new byte[128 / 8];
				new Random().NextBytes(_entropy);
			}
		}

		/// <summary>
		/// Returns an initialized scheme with default / suggested parameters
		/// </summary>
		/// <param name="parameter">Optional numeric parameter to the scheme</param>
		public abstract S GetScheme(int parameter = 0);
	}

	public class NoEncryptionFactory : ORESchemesFactory<NoEncryptionScheme, OPECipher, BytesKey>
	{
		public NoEncryptionFactory(int? seed = null) : base(seed) { }

		public override NoEncryptionScheme GetScheme(int parameter = 0) => new NoEncryptionScheme(_entropy);
	}

	public class PracticalOREFactory : ORESchemesFactory<PracticalOREScheme, ORESchemes.PracticalORE.Ciphertext, BytesKey>
	{
		public PracticalOREFactory(int? seed = null) : base(seed) { }

		public override PracticalOREScheme GetScheme(int parameter = 0) => new PracticalOREScheme(_entropy);
	}

	public class CryptDBOPEFactory : ORESchemesFactory<CryptDBScheme, OPECipher, BytesKey>
	{
		public CryptDBOPEFactory(int? seed = null) : base(seed) { }

		public override CryptDBScheme GetScheme(int parameter = 48) =>
			new CryptDBScheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(-Math.Pow(2, parameter)),
				Convert.ToInt64(Math.Pow(2, parameter)),
				_entropy
			);
	}

	public class LewiOREFactory : ORESchemesFactory<LewiOREScheme, ORESchemes.LewiORE.Ciphertext, ORESchemes.LewiORE.Key>
	{
		public LewiOREFactory(int? seed = null) : base(seed) { }

		public override LewiOREScheme GetScheme(int parameter = 16) => new LewiOREScheme(parameter, _entropy);
	}

	public class FHOPEFactory : ORESchemesFactory<FHOPEScheme, ORESchemes.FHOPE.Ciphertext, State>
	{
		public FHOPEFactory(int? seed = null) : base(seed) { }

		public override FHOPEScheme GetScheme(int parameter = 0) => new FHOPEScheme(long.MinValue, long.MaxValue, 10, parameter * 0.01, _entropy);
	}

	public class AdamOREFactory : ORESchemesFactory<AdamOREScheme, ORESchemes.AdamORE.Ciphertext, ORESchemes.AdamORE.Key>
	{
		public AdamOREFactory(int? seed = null) : base(seed) { }

		public override AdamOREScheme GetScheme(int parameter = 0) => new AdamOREScheme(_entropy);
	}
}
