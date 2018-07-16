using System;
using System.Collections;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRP;
using Xunit;

namespace Test.ORESchemes.Primitives.PRP
{
	[Trait("Category", "Unit")]
	public class FeistelPRP : AbsPRP
	{
		public FeistelPRP() : base(new Feistel(3)) { }

		[Fact]
		public void Factory()
		{
			var prp = new PRPFactory().GetPrimitive();

			Assert.NotNull(prp);
			Feistel feistel = Assert.IsType<Feistel>(prp);
			Assert.Equal(3, feistel.Rounds);
		}

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRP>(
				_prp,
				(P) =>
				{
					for (int i = 0; i < 8; i++)
					{
						var input = new BitArray(new int[] { i });
						P.PRP(input, _key, 3);
						P.InversePRP(input, _key, 3);
					}
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 48 },
					{ Primitive.PRP, 16 },
					{ Primitive.AES, 48 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRP, 16 }
				}
			);
		}
	}

	[Trait("Category", "Integration")]
	public class StrongFeistel : AbsPRP
	{
		public StrongFeistel() : base(new Feistel(4)) { }

		[Fact]
		public void Factory()
		{
			var prp = new StrongPRPFactory().GetPrimitive();

			Assert.NotNull(prp);
			Feistel feistel = Assert.IsType<Feistel>(prp);
			Assert.Equal(4, feistel.Rounds);
		}

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRP>(
				_prp,
				(P) =>
				{
					for (int i = 0; i < 8; i++)
					{
						var input = new BitArray(new int[] { i });
						P.PRP(input, _key, 3);
						P.InversePRP(input, _key, 3);
					}
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 64 },
					{ Primitive.PRP, 16 },
					{ Primitive.AES, 64 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRP, 16 }
				}
			);
		}
	}

	public abstract class AbsPRP
	{
		private const int RUNS = 100;
		private const int SEED = 123456;
		protected readonly byte[] _key = new byte[128 / 8];

		protected readonly IPRP _prp;

		public AbsPRP(IPRP prp)
		{
			new Random(SEED).NextBytes(_key);

			_prp = prp;
		}

		[Fact]
		public void OneToOne()
		{
			var set = new HashSet<BitArray>();

			for (int i = -RUNS; i < RUNS; i++)
			{
				set.Add(_prp.PRP(new BitArray(new int[] { i }), _key));
			}

			Assert.Equal(2 * RUNS, set.Count);
		}

		[Fact]
		public void NoIdentity()
		{
			int identities = 0;

			for (int i = -RUNS; i < RUNS; i++)
			{
				var input = new BitArray(new int[] { i });
				if (_prp.PRP(input, _key) == input)
				{
					identities++;
				}
			}

			Assert.InRange(identities, 0, 2 * RUNS * 0.01);
		}

		[Fact]
		public void OddBits()
		{
			var set = new HashSet<byte>();

			for (byte i = 0; i < 2; i++)
			{
				for (byte j = 0; j < 2; j++)
				{
					for (byte k = 0; k < 2; k++)
					{
						var input = new BitArray(new bool[] { i % 2 == 0, j % 2 == 0, k % 2 == 0 });
						var output = _prp.PRP(input, _key);

						byte[] number = new byte[1];
						output.CopyTo(number, 0);

						set.Add(number[0]);
					}
				}
			}

			Assert.Equal(8, set.Count);

			for (byte i = 0; i < 8; i++)
			{
				Assert.Contains(i, set);
			}
		}

		[Fact]
		/// <summary>
		/// Idea here is that we supply number of bits explicitly and expect
		/// the algorithm use this number, not a length of array.
		/// This test is a response to an actual bug.
		/// </summary>
		public void ProperLengthUsed()
		{
			var set = new HashSet<int>();

			for (int i = 0; i < 8; i++)
			{
				var input = new BitArray(new int[] { i });
				var output = _prp.PRP(input, _key, 3);

				int[] number = new int[1];
				output.CopyTo(number, 0);

				set.Add(number[0]);
			}

			Assert.Equal(8, set.Count);

			for (byte i = 0; i < 8; i++)
			{
				Assert.Contains(i, set);
			}
		}
	}
}
