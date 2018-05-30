using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
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
		byte[] ComputeHash(byte[] input);

		byte[] ComputeHash(byte[] input, byte[] key);
	}

	public abstract class AbsHash : IHash
	{
		public abstract byte[] ComputeHash(byte[] input);

		public virtual byte[] ComputeHash(byte[] input, byte[] key) =>
			ComputeHash(PRFFactory.GetPRF().PRF(key, input, new byte[] { 0x00 }));
	}

	public class SHA256 : AbsHash
	{
		public override byte[] ComputeHash(byte[] input) =>
			new SHA256Managed().ComputeHash(input);
	}
}
