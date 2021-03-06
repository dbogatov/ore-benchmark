
using System;
using Crypto.FHOPE;
using Crypto.Shared.Primitives.PRG;
using Xunit;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class FHOPEState
	{
		private readonly State _state;

		public FHOPEState()
		{
			byte[] entropy = new byte[128 / 8];
			new Random(123456).NextBytes(entropy);

			_state = new State(new PRGCachedFactory(entropy).GetPrimitive(), 0, ulong.MaxValue, 10, 0);
		}

		[Fact]
		public void SizeZero()
			=> Assert.Equal(0, _state.GetSize());

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
