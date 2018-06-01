using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRP;
using Xunit;

namespace Test.ORESchemes.Primitives
{
	[Trait("Category", "Unit")]
	public class FeistelTests : AbsPRPTests
	{
		public FeistelTests() : base(new Feistel(3)) { }
	}

	[Trait("Category", "Unit")]
	public class StrongFeistelTests : AbsPRPTests
	{
		public StrongFeistelTests() : base(new Feistel(4)) { }
	}

	public abstract class AbsPRPTests
	{
		private const int RUNS = 1000;
		private const int SEED = 123456;
		private readonly byte[] _key = new byte[256 / 8];

		private readonly IPRP _prp;

		public AbsPRPTests(IPRP prp)
		{
			new Random(SEED).NextBytes(_key);

			_prp = prp;
		}

		[Fact]
		public void OneToOneTest()
		{
			var set = new HashSet<BitArray>();

			for (int i = -RUNS; i < RUNS; i++)
			{
				set.Add(_prp.PRP(new BitArray(new int[] { i }), _key));
			}

			Assert.Equal(2 * RUNS, set.Count);
		}

		[Fact]
		public void NoIdentityTest()
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
		public void OddBitsTest()
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
	}

	// public class TestPRP
	// {
	// 	private const int RUNS = 1000;
	// 	private const int SEED = 123456;
	// 	private readonly byte[] _key = new byte[256 / 8];

	// 	private readonly IPRP _prp;

	// 	public TestPRP()
	// 	{
	// 		new Random(SEED).NextBytes(_key);

	// 		_prp = new Feistel(3);
	// 	}


	// 	[Fact]
	// 	public void OneToOneTest()
	// 	{
	// 		var set = new HashSet<BitArray>();

	// 		for (int i = -RUNS; i < RUNS; i++)
	// 		{
	// 			set.Add(_prp.PRP(new BitArray(new int[] { i }), _key));
	// 		}

	// 		Assert.Equal(2 * RUNS, set.Count);
	// 	}

	// 	[Fact]
	// 	public void NoIdentityTest()
	// 	{
	// 		int identities = 0;

	// 		for (int i = -RUNS; i < RUNS; i++)
	// 		{
	// 			var input = new BitArray(new int[] { i });
	// 			if (_prp.PRP(input, _key) == input)
	// 			{
	// 				identities++;
	// 			}
	// 		}

	// 		Assert.InRange(identities, 0, 2 * RUNS * 0.01);
	// 	}

	// 	[Fact]
	// 	public void OddBitsTest()
	// 	{
	// 		var set = new HashSet<byte>();

	// 		for (byte i = 0; i < 2; i++)
	// 		{
	// 			for (byte j = 0; j < 2; j++)
	// 			{
	// 				for (byte k = 0; k < 2; k++)
	// 				{
	// 					var input = new BitArray(new bool[] { i % 2 == 0, j % 2 == 0, k % 2 == 0 });
	// 					var output = _prp.PRP(input, _key);

	// 					byte[] number = new byte[1];
	// 					output.CopyTo(number, 0);

	// 					set.Add(number[0]);
	// 				}
	// 			}
	// 		}

	// 		Assert.Equal(8, set.Count);

	// 		for (byte i = 0; i < 8; i++)
	// 		{
	// 			Assert.Contains(i, set);
	// 		}
	// 	}
	// }
}
