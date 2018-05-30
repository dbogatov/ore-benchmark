using System;
using System.Collections.Generic;
using System.Text;
using ORESchemes.Shared.Primitives;
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

		private readonly IPRP<int> _prp;

		public AbsPRPTests(IPRP<int> prp)
		{
			new Random(SEED).NextBytes(_key);

			_prp = prp;
		}

		[Fact]
		public void OneToOneTest()
		{
			var set = new HashSet<int>();

			for (int i = -RUNS * 100; i < RUNS * 100; i++)
			{
				set.Add(_prp.PRP(i, _key));
			}

			Assert.Equal(2 * RUNS * 100, set.Count);
		}

		[Fact]
		public void CorrectnessTest()
		{
			for (int i = -RUNS * 100; i < RUNS * 100; i++)
			{
				int encrypted = _prp.PRP(i, _key);
				int decrypted = _prp.InversePRP(encrypted, _key);
				Assert.Equal(i, decrypted);
			}
		}
	}
}
