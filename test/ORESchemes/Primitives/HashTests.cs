using System;
using System.Collections.Generic;
using System.Text;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared;
using Xunit;
using ORESchemes.Shared.Primitives.Hash;

namespace Test.ORESchemes.Primitives.Hash
{
	[Trait("Category", "Unit")]
	public class SHA256Tests : AbsHashTests
	{
		public SHA256Tests() : base(new SHA256()) { }

		[Theory]
		[InlineData("Hello, world", "4AE7C3B6AC0BEFF671EFA8CF57386151C06E58CA53A78D83F36107316CEC125F")]
		[InlineData("", "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855")]
		[InlineData("1305", "C7F4F146491D27EED1CD4D422C9B3FEC37006C606B17DABBFEBEF85845F3CBC1")]
		public void CorrectnessTest(string input, string output) =>
			Assert.Equal(output, _hash.ComputeHash(Encoding.Default.GetBytes(input)).PrintHex());
	}

	public abstract class AbsHashTests
	{
		protected readonly IHash _hash;
		private const int SEED = 123456;
		private readonly byte[] _key = new byte[256 / 8];
		private readonly byte[] _anotherKey = new byte[256 / 8];
		private const int RUNS = 1000;

		public AbsHashTests(IHash hash)
		{
			Random random = new Random(SEED);
			random.NextBytes(_key);
			random.NextBytes(_anotherKey);

			_hash = hash;
		}

		[Fact]
		public void DeterministicTest()
		{
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(
					_hash.ComputeHash(BitConverter.GetBytes(i)),
					_hash.ComputeHash(BitConverter.GetBytes(i))
				);
			}
		}

		[Fact]
		public void NoDuplicatesTest()
		{
			HashSet<byte[]> set = new HashSet<byte[]>();

			for (int i = 0; i < RUNS * 100; i++)
			{
				set.Add(_hash.ComputeHash(BitConverter.GetBytes(i)));
			}

			Assert.Equal(RUNS * 100, set.Count);
		}

		[Fact]
		public void DeterministicSameKeyTest()
		{
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(
					_hash.ComputeHash(BitConverter.GetBytes(i), _key),
					_hash.ComputeHash(BitConverter.GetBytes(i), _key)
				);
			}
		}

		[Fact]
		public void DifferentKeyTest()
		{
			for (int i = 0; i < RUNS; i++)
			{
				Assert.NotEqual(
					_hash.ComputeHash(BitConverter.GetBytes(i), _key),
					_hash.ComputeHash(BitConverter.GetBytes(i), _anotherKey)
				);
			}
		}
	}
}