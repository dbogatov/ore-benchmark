using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	public class LFPRFFactory
	{
		/// <summary>
		/// Returns an initialized instance of a PRG
		/// </summary>
		public static ILFPRF GetLFPRF(Nullable<int> seed = null)
		{
			return new TapeGen(
				PRFFactory.GetPRF(),
				PRGFactory.GetPRG(seed)
			);
		}
	}

	public interface ILFPRF
	{
		byte[] Generate(byte[] key, int length, byte[] input);
	}


	public class TapeGen : ILFPRF
	{
		public IPRF _prf;
		public IPRG _prg;

		public TapeGen(IPRF prf, IPRG prg)
		{
			_prf = prf;
			_prg = prg;
		}

		public byte[] Generate(byte[] key, int length, byte[] input)
		{
			byte[] result = new byte[length];

			byte[] encrypted = _prf.PRF(key, input, key);

			// TODO
			// Seed should be the entire bytes from PRF
			// Not reduced entropy
			int seed = BytesHashCode(encrypted);

			_prg
				.RecreateWithSeed(seed)
				.NextBytes(result);

			return result;
		}

		// https://stackoverflow.com/a/7244316/1644554
		private int BytesHashCode(byte[] bytes)
		{
			var hashCode = 0;
			for (var i = 0; i < bytes.Length; i++)
			{
				// Rotate by 3 bits and XOR the new value.
				hashCode = (hashCode << 3) | (hashCode >> (29)) ^ bytes[i];
			}
			return hashCode;
		}
	}
}
