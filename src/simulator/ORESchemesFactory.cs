using System;
using ORESchemes.Shared;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;

namespace Simulation
{
	public abstract class ORESchemesFactory<C>
	{
		/// <summary>
		/// Returns an initialized scheme
		/// </summary>
		/// <param name="scheme">Enum indicating the requested scheme</param>
		/// <returns>Initialized scheme</returns>
		/// <remarks>
		/// Will throw exception if requested scheme is not of the proper type
		/// </remarks>
		public abstract IOREScheme<C> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null);
	}

	public class OPESchemesFactory : ORESchemesFactory<long>
	{
		public override IOREScheme<long> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null)
		{
			byte[] entropy;
			if (seed != null)
			{
				entropy = BitConverter.GetBytes(seed.Value);
			}
			else
			{
				entropy = new byte[256 / 8];
				new Random().NextBytes(entropy);
			}

			IOREScheme<long> result;
			switch (scheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					result = new NoEncryptionScheme(entropy);
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					result = new CryptDBScheme(
						Int32.MinValue,
						Int32.MaxValue,
						Convert.ToInt64(Int32.MinValue) * 100000, // reasonable maximum
						Convert.ToInt64(Int32.MaxValue) * 100000, // larger numbers cause performance degradation
						entropy
					);
					break;
				default:
					throw new ArgumentException($"{scheme} scheme is not an OPE scheme.");
			}

			result.Init();
			return result;
		}
	}

	public class ORESchemesFactoryPractical : ORESchemesFactory<ORESchemes.PracticalORE.Ciphertext>
	{
		public override IOREScheme<Ciphertext> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null)
		{
			byte[] entropy;
			if (seed != null)
			{
				entropy = BitConverter.GetBytes(seed.Value);
			}
			else
			{
				entropy = new byte[256 / 8];
				new Random().NextBytes(entropy);
			}

			IOREScheme<Ciphertext> result;
			if (scheme == ORESchemes.Shared.ORESchemes.PracticalORE)
			{
				result = new PracticalOREScheme(entropy);
			}
			else
			{
				throw new ArgumentException($"{scheme} scheme is not PracticalORE");
			}

			result.Init();
			return result;
		}
	}
}

