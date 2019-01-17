using System;

namespace Packages
{
	partial class Program
	{
		static private void ORESchemes()
		{
			void UseScheme<C, K>(
				ORESchemes.Shared.IOREScheme<C, K> scheme,
				Action<C, ORESchemes.Shared.IOREScheme<C, K>, int, K> preprocess = null
			)
				where C : ORESchemes.Shared.IGetSize
				where K : ORESchemes.Shared.IGetSize
			{
				// Generate a key
				var key = scheme.KeyGen();

				// Encrypt some numbers
				var cipher5 = scheme.Encrypt(5, key);
				var cipher6 = scheme.Encrypt(6, key);

				// If FH-OPE then we need max and min for ciphers to compare
				if (preprocess != null)
				{
					preprocess(cipher5, scheme, 5, key);
					preprocess(cipher6, scheme, 6, key);
				}

				scheme.IsLess(cipher5, cipher6); // true
				scheme.IsLessOrEqual(cipher5, cipher6); // true
				scheme.IsEqual(cipher5, cipher6); // false
				scheme.IsGreaterOrEqual(cipher5, cipher6); // false
				scheme.IsGreater(cipher5, cipher6); // false

				int plaint5 = scheme.Decrypt(cipher5, key); // 5
				int plaint6 = scheme.Decrypt(cipher6, key); // 6
			}


			UseScheme(
				new ORESchemes.CryptDBOPE.CryptDBScheme(
					int.MinValue,
					int.MaxValue,
					Convert.ToInt64(-Math.Pow(2, 48)),
					Convert.ToInt64(Math.Pow(2, 48))
				)
			);

			UseScheme(
				new ORESchemes.PracticalORE.PracticalOREScheme()
			);

			UseScheme(
				new ORESchemes.LewiORE.LewiOREScheme()
			);

			UseScheme(
				new ORESchemes.FHOPE.FHOPEScheme(long.MinValue, long.MaxValue),
				(cipher, scheme, plain, key) =>
				{
					cipher.max = ((ORESchemes.FHOPE.FHOPEScheme)scheme).MaxCiphertext(plain, key);
					cipher.min = ((ORESchemes.FHOPE.FHOPEScheme)scheme).MinCiphertext(plain, key);
				}
			);
		}
	}
}
