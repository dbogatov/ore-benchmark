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
		Key KeyGen();

		byte[] Hash(byte[] hashKey, byte[] input);

		bool Test(byte[] testKey, byte[] inputOne, byte[] inputTwo);
	}

	public class Key : IGetSize
	{
		public byte[] hashKey;
		public byte[] testKey;

		public int GetSize() => 8 * (hashKey.Length + testKey.Length);
	}

	public class FakeGenericPPH : AbsPrimitive, IPPH
	{
		private readonly IPRG G;
		private readonly Func<byte[], byte[], bool> PR;

		public FakeGenericPPH(byte[] entropy, Func<byte[], byte[], bool> predicate)
		{
			G = new PRGFactory(entropy).GetPrimitive();
			PR = predicate;
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

	public class FakePPH : FakeGenericPPH
	{
		public FakePPH(byte[] entropy) :
			base(entropy, (one, two) => new BigInteger(one) == new BigInteger(two) + 1)
		{ }
	}
}
