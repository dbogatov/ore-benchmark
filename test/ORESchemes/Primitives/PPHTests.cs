using System;
using System.Collections.Generic;
using System.Numerics;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PPH;
using Xunit;

namespace Test.ORESchemes.Primitives.PPH
{
	[Trait("Category", "Unit")]
	public class FakePPH
	{
		private readonly IPPH R;
		private readonly Random G = new Random(SEED);
		static readonly int SEED = 123456;
		private readonly int RUNS = 1000;
		private readonly Key _key;

		public FakePPH()
		{
			byte[] entropy = new byte[128 / 8];
			G.NextBytes(entropy);

			R = new global::ORESchemes.Shared.Primitives.PPH.FakePPH(entropy);
			_key = R.KeyGen();
		}

		[Fact]
		public void PropertyPreserved()
		{
			for (int i = 0; i < RUNS; i++)
			{
				byte[] input = new byte[128 / 8];
				G.NextBytes(input);

				byte[] greater = (new BigInteger(input) + 1).ToByteArray();
				byte[] smaller = (new BigInteger(input) - 1).ToByteArray();

				Assert.True(
					R.Test(
						_key.testKey,
						R.Hash(_key.hashKey, greater),
						R.Hash(_key.hashKey, input)
					)
				);

				Assert.True(
					R.Test(
						_key.testKey,
						R.Hash(_key.hashKey, input),
						R.Hash(_key.hashKey, smaller)
					)
				);
			}
		}

		[Fact]
		public void NoFalsePositives()
		{
			for (int i = 0; i < RUNS; i++)
			{
				byte[] input = new byte[128 / 8];
				byte[] other = new byte[128 / 8];
				G.NextBytes(input);
				G.NextBytes(other);

				Assert.False(
					R.Test(
						_key.testKey,
						R.Hash(_key.hashKey, other),
						R.Hash(_key.hashKey, input)
					)
				);
			}
		}

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPPH>(
				R,
				pph =>
				{
					var key = pph.KeyGen();
					pph.Test(
						key.testKey,
						pph.Hash(key.hashKey, new byte[] { }),
						pph.Hash(key.hashKey, new byte[] { })
					);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PPH, 3 },
					{ Primitive.PRG, 2 },
					{ Primitive.AES, 2 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PPH, 3 }
				}
			);
		}
	}
}
