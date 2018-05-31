using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ORESchemes.Shared.Primitives.PRF;

namespace ORESchemes.Shared.Primitives.Hash
{
	public class HashFactory
	{
		/// <summary>
		/// Returns an initialized instance of a Hash function
		/// </summary>
		public static IHash GetHash()
		{
			return new SHA256();
		}
	}

	public interface IHash
	{
		/// <summary>
		/// Returns the hash value of the input
		/// </summary>
		/// <param name="input">Input to hash function</param>
		/// <returns>Hash value</returns>
		byte[] ComputeHash(byte[] input);

		/// <summary>
		/// Returns the hash value of the input put through a PRF using the key
		/// </summary>
		/// <param name="input">Input to hash function</param>
		/// <param name="key">Key to the function (source of randomness)</param>
		/// <returns>Hash value</returns>
		byte[] ComputeHash(byte[] input, byte[] key);
	}

	public abstract class AbsHash : IHash
	{
		public abstract byte[] ComputeHash(byte[] input);

		public virtual byte[] ComputeHash(byte[] input, byte[] key) =>
			ComputeHash(PRFFactory.GetPRF().DeterministicPRF(key, input));
	}

	public class SHA256 : AbsHash
	{
		public override byte[] ComputeHash(byte[] input) =>
			new SHA256Managed().ComputeHash(input);
	}
}
