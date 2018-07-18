using System;
using System.Numerics;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.Shared.Primitives.PPH
{
	public class PPHFactory : AbsPrimitiveFactory<IPPH>
	{
		public PPHFactory(byte[] entropy = null) : base(entropy) { }

		protected override IPPH CreatePrimitive(byte[] entropy) => new FakePPH(entropy);
	}

	public interface IPPH : IPrimitive
	{
		/// <summary>
		/// Generates PPH key - a pair hash key and test key
		/// </summary>
		Key KeyGen();

		/// <summary>
		/// Produces a property preserving hash of its input
		/// </summary>
		/// <param name="hashKey">Key to the hash function</param>
		/// <param name="input">Input ti the hash function</param>
		/// <returns>The hash of the input</returns>
		byte[] Hash(byte[] hashKey, byte[] input);

		/// <summary>
		/// Tests if given two inputs pass the predicate (property)
		/// </summary>
		/// <param name="testKey">Public test key</param>
		/// <param name="inputOne">First input</param>
		/// <param name="inputTwo">Second input</param>
		/// <returns>True, if predicate passes, false otherwise</returns>
		bool Test(byte[] testKey, byte[] inputOne, byte[] inputTwo);
	}

	public class Key : IGetSize
	{
		public byte[] hashKey;
		public byte[] testKey;

		public int GetSize() => 8 * (hashKey.Length + testKey.Length);
	}

	/// <summary>
	/// This a generic fake PPH where user can manually supply the predicate
	/// WARNING: this is fake, so no security at all, only correctness
	/// </summary>
	public class FakeGenericPPH : AbsPrimitive, IPPH
	{
		private readonly IPRG G;
		private readonly Func<byte[], byte[], bool> PR;

		public FakeGenericPPH(byte[] entropy, Func<byte[], byte[], bool> predicate)
		{
			G = new PRGFactory(entropy).GetPrimitive();

			PR = predicate;

			G.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) => base.OnUse(prim, true)
			);
		}

		public Key KeyGen() => new Key
		{
			hashKey = G.GetBytes(128 / 8),
			testKey = G.GetBytes(128 / 8)
		};

		public byte[] Hash(byte[] hashKey, byte[] input)
		{
			OnUse(Primitive.PPH);

			return input;
		}

		public bool Test(byte[] testKey, byte[] inputOne, byte[] inputTwo)
		{
			OnUse(Primitive.PPH);

			return PR(inputOne, inputTwo);
		}
	}

	/// <summary>
	/// Particualt implementation of generic fake PPH where predicate is x = y + 1
	/// WARNING: this is fake, so no security at all, only correctness
	/// </summary>
	public class FakePPH : FakeGenericPPH
	{
		public FakePPH(byte[] entropy) :
			base(entropy, (one, two) => new BigInteger(one) == new BigInteger(two) + 1)
		{ }
	}
}
