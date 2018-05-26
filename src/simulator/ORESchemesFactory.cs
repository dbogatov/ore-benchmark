using System;
using ORESchemes.Shared;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;

namespace Simulation
{
	public abstract class ORESchemesFactory<P, C>
	{
		/// <summary>
		/// Returns an initialized scheme
		/// </summary>
		/// <param name="scheme">Enum indicating the requested scheme</param>
		/// <returns>Initialized scheme</returns>
		/// <remarks>
		/// Will throw exception if requested scheme is not of the proper type
		/// </remarks>
		public abstract IOREScheme<P, C> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null);
	}

	public class ORESchemesFactoryIntToInt : ORESchemesFactory<int, long>
	{
		public override IOREScheme<int, long> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null)
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

			IOREScheme<int, long> result;
			switch (scheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					result = new NoEncryptionScheme(entropy);
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					result = new CryptDBScheme(
						Int32.MinValue,
						Int32.MaxValue,
						unchecked((long)(Int32.MinValue * 10)), // empirical maximum
						unchecked((long)(Int32.MaxValue * 10)),
						entropy
					);
					break;
				default:
					throw new ArgumentException($"{scheme} scheme is not Int to Long");
			}

			result.Init();
			return result;
		}
	}

	public class ORESchemesFactoryPractical : ORESchemesFactory<int, ORESchemes.PracticalORE.Ciphertext>
	{
		public override IOREScheme<int, Ciphertext> GetScheme(ORESchemes.Shared.ORESchemes scheme, int? seed = null)
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

			IOREScheme<int, Ciphertext> result;
			switch (scheme)
			{
				case ORESchemes.Shared.ORESchemes.PracticalORE:
					result = new PracticalOREScheme(entropy);
					break;
				default:
					throw new ArgumentException($"{scheme} scheme is not PracticalORE");
			}

			result.Init();
			return result;
		}
	}
}

