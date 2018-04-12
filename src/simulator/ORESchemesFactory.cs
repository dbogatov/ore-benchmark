using System;
using ORESchemes.Shared;
using ORESchemes.CryptDBOPE;
using ORESchemes.PracticalORE;

namespace Simulation
{
	public class ORESchemesFactoryIntToInt
	{
		/// <summary>
		/// Returns an initialized scheme
		/// </summary>
		/// <param name="scheme">Enum indicating the requested scheme</param>
		/// <returns>Initialized scheme</returns>
		public static IOREScheme<int, int> GetScheme(ORESchemes.Shared.ORESchemes scheme, int alpha = 128, int seed = 0)
		{
			seed = seed == 0 ? new Random().Next(Int32.MaxValue) : seed;

			IOREScheme<int, int> result;
			switch (scheme)
			{
				case ORESchemes.Shared.ORESchemes.NoEncryption:
					result = new NoEncryptionScheme();
					break;
				case ORESchemes.Shared.ORESchemes.CryptDB:
					result = new CryptDBScheme();
					break;
				// case ORESchemes.Shared.ORESchemes.PracticalORE:
				// 	result = new PracticalOREScheme(alpha, seed);
				// 	break;
				default:
					throw new ArgumentException("Scheme enum is invalid");
			}

			result.Init();
			return result;
		}
	}
}
