using System;
using Xunit;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

namespace Test
{
	[Trait("Category", "Unit")]
	public class Miscellaneous
	{
		[Fact]
		public void PrintByteExtension()
		{
			var bytes = new byte[] { 0x00, 0x13, 0x05, 0x19, 0x96, 0xAA };

			var description = bytes.Print();

			foreach (var b in bytes)
			{
				Assert.Contains(b.ToString(), description);
			}
		}

		[Fact]
		public void RecordPrintMethods()
		{
			Random generator = new Random(123456);

			for (int i = 0; i < 10; i++)
			{
				var index = generator.Next();
				var value = generator.Next().ToString();
				Simulation.Protocol.Record record = new Simulation.Protocol.Record(index, value);
				Assert.Contains(index.ToString(), record.ToString());
				Assert.Contains(value.ToString(), record.ToString());

				Simulation.Protocol.RangeQuery query = new Simulation.Protocol.RangeQuery(index, 2 * index);
				Assert.Contains(index.ToString(), query.ToString());
				Assert.Contains((index * 2).ToString(), query.ToString());
			}
		}

		[Fact]
		public void GetBytesExtension()
		{
			Random random = new Random(123456);
			byte[] entropy = new byte[128 / 8];
			random.NextBytes(entropy);

			IPRG G1 = new PRGFactory(entropy).GetPrimitive();
			IPRG G2 = new PRGFactory(entropy).GetPrimitive();

			byte[] bytes1 = new byte[128 / 8];
			G1.NextBytes(bytes1);

			byte[] bytes2 = G2.GetBytes(128 / 8);

			Assert.Equal(bytes1, bytes2);
		}

		[Fact]
		public void OPECipherImplicitConversions()
		{
			Random random = new Random(123456);

			for (int i = 0; i < 100; i++)
			{
				var a = random.Next();
				var b = i % 20 == 0 ? a : random.Next(); // once in a while let them be equal

				OPECipher A = a;
				OPECipher B = b;

				Assert.True((a < b) == (A < B));
				Assert.True((a > b) == (A > B));
				Assert.True((a == b) == (A == B));
				Assert.True((a <= b) == (A <= B));
				Assert.True((a >= b) == (A >= B));

				Assert.True((A == B) == (A.GetHashCode() == B.GetHashCode()));
				Assert.True((A == B) == A.Equals(B));

				Assert.True(A.value == (long)A);
				Assert.True(B.value == (long)B);
			}
		}

		[Fact]
		public void BytesKeyImplicitConversions()
		{
			Random random = new Random(123456);

			for (int i = 0; i < 100; i++)
			{
				byte[] a = new byte[128 / 8];
				byte[] b = new byte[128 / 8];

				random.NextBytes(a);
				if (i % 20 == 0)
				{
					b = a;
				}
				else
				{
					random.NextBytes(b);
				}

				BytesKey A = a;
				BytesKey B = b;

				Assert.True(A.value == (byte[])A);
				Assert.True(B.value == (byte[])B);
			}
		}
	}
}
