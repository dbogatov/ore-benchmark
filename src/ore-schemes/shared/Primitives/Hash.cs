using System.Linq;
using System.Security.Cryptography;
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

	public interface IHash : IPrimitive
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

	public abstract class AbsHash : AbsPrimitive, IHash
	{
		IPRF F;

		public AbsHash()
		{
			F = PRFFactory.GetPRF();

			F.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) => base.OnUse(prim, true)
			);
		}

		public abstract byte[] ComputeHash(byte[] input);

		public virtual byte[] ComputeHash(byte[] input, byte[] key) =>
			ComputeHash(
				F.PRF(
					key,
					input,
					Enumerable.Repeat((byte)0x00, 128 / 8).ToArray()
				).Skip(128 / 8).ToArray()
			);
	}

	public class SHA256 : AbsHash
	{
		public override byte[] ComputeHash(byte[] input)
		{
			OnUse(Primitive.Hash);

			return new SHA256Managed().ComputeHash(input);
		}
	}
}
