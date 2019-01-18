using System;
using Crypto.Shared;
using Crypto.BCLO;
using Crypto.CLWW;
using Crypto.LewiWu;
using Crypto.FHOPE;
using Crypto.CLOZ;

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

	public class CLWWFactory : ORESchemesFactory<Crypto.CLWW.Scheme, Crypto.CLWW.Ciphertext, BytesKey>
	{
		public CLWWFactory(int? seed = null) : base(seed) { }

		public override Crypto.CLWW.Scheme GetScheme(int parameter = 0) => new Crypto.CLWW.Scheme(_entropy);
	}

	public class BCLOFactory : ORESchemesFactory<Crypto.BCLO.Scheme, OPECipher, BytesKey>
	{
		public BCLOFactory(int? seed = null) : base(seed) { }

		public override Crypto.BCLO.Scheme GetScheme(int parameter = 48) =>
			new Crypto.BCLO.Scheme(
				Int32.MinValue,
				Int32.MaxValue,
				Convert.ToInt64(-Math.Pow(2, parameter)),
				Convert.ToInt64(Math.Pow(2, parameter)),
				_entropy
			);
	}

	public class LewiWuFactory : ORESchemesFactory<Crypto.LewiWu.Scheme, Crypto.LewiWu.Ciphertext, Crypto.LewiWu.Key>
	{
		public LewiWuFactory(int? seed = null) : base(seed) { }

		public override Crypto.LewiWu.Scheme GetScheme(int parameter = 16) => new Crypto.LewiWu.Scheme(parameter, _entropy);
	}

	public class FHOPEFactory : ORESchemesFactory<Crypto.FHOPE.Scheme, Crypto.FHOPE.Ciphertext, State>
	{
		public FHOPEFactory(int? seed = null) : base(seed) { }

		public override Crypto.FHOPE.Scheme GetScheme(int parameter = 0) => new Crypto.FHOPE.Scheme(long.MinValue, long.MaxValue, 10, parameter * 0.01, _entropy);
	}

	public class CLOZFactory : ORESchemesFactory<Crypto.CLOZ.Scheme, Crypto.CLOZ.Ciphertext, Crypto.CLOZ.Key>
	{
		public CLOZFactory(int? seed = null) : base(seed) { }

		public override Crypto.CLOZ.Scheme GetScheme(int parameter = 0) => new Crypto.CLOZ.Scheme(_entropy);
	}
}
