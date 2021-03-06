using System.Security.Cryptography;
using Crypto.Shared.Primitives.PRF;

namespace Crypto.Shared.Primitives.Hash
{
	public class HashFactory : AbsPrimitiveFactory<IHash>
	{
		protected override IHash CreatePrimitive(byte[] entropy) => new SHA256();
	}

	public class Hash512Factory : AbsPrimitiveFactory<IHash>
	{
		protected override IHash CreatePrimitive(byte[] entropy) => new SHA512();
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
			F = new PRFFactory().GetPrimitive();

			RegisterPrimitive(F);
		}

		public abstract byte[] ComputeHash(byte[] input);

		public virtual byte[] ComputeHash(byte[] input, byte[] key) => ComputeHash(F.PRF(key, input));
	}

	public class SHA256 : AbsHash
	{
		public override byte[] ComputeHash(byte[] input)
		{
			OnUse(Primitive.Hash);

			return new SHA256Managed().ComputeHash(input);
		}
	}

	public class SHA512 : AbsHash
	{
		public override byte[] ComputeHash(byte[] input)
		{
			OnUse(Primitive.Hash);

			return new SHA512Managed().ComputeHash(input);
		}
	}
}
