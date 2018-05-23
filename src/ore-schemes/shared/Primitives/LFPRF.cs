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
		public static ILFPRF GetLFPRF()
		{
			return new TapeGen();
		}
	}

	public interface ILFPRF
	{
		byte[] Generate(byte[] key, int length, byte[] input);
	}


	public class TapeGen : ILFPRF
	{
		public byte[] Generate(byte[] key, int length, byte[] input)
		{
			byte[] result = new byte[length];

			byte[] encrypted = PRFFactory.GetPRF().PRF(key, input, key);

			// TODO
			// Seed should be the entire bytes from PRF
			// Not reduced entropy
			// int seed = encrypted.GetProperHashCode();

			PRGFactory.GetPRG(encrypted).NextBytes(result);

			return result;
		}
	}
}
