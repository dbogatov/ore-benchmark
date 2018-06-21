
using System;
using ORESchemes.FHOPE;
using ORESchemes.Shared.Primitives.PRG;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class FHOPEStateTests
	{
		private readonly State _state;

		public FHOPEStateTests()
		{
			byte[] entropy = new byte[256 / 8];
			new Random(123456).NextBytes(entropy);

			_state = new State(PRGFactory.GetDefaultPRG(entropy), 0, ulong.MaxValue, 10, 0);
		}

		[Fact]
		public void SizeZero()
		{
			Assert.Equal(0, _state.GetSize());
		}

		[Fact]
		public void SizeFewElements()
		{
			_state.Insert(1);
			_state.Insert(2);
			_state.Insert(5);

			Assert.Equal((3 * sizeof(int) + 2 * sizeof(long)) * 8, _state.GetSize());
		}

		[Fact]
		public void SizeCluster()
		{
			_state.Insert(2);
			_state.Insert(1);

			_state.Insert(5);
			_state.Insert(5);
			_state.Insert(5);

			Assert.Equal((6 * sizeof(int) + 2 * sizeof(long)) * 8, _state.GetSize());
		}

		[Fact]
		public void SizeComplex()
		{
			_state.Insert(2);
			_state.Insert(1);

			_state.Insert(-5);
			_state.Insert(-5);
			_state.Insert(-5);
			_state.Insert(-5);

			_state.Insert(5);
			_state.Insert(5);
			_state.Insert(5);

			Assert.Equal((11 * sizeof(int) + 3 * sizeof(long)) * 8, _state.GetSize());
		}
	}
}
