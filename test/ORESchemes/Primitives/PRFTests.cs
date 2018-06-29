using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRP;
using Xunit;

namespace Test.ORESchemes.Primitives.PRF
{
	[Trait("Category", "Unit")]
	public class AESPRF : AbsPRF
	{
		public AESPRF() : base(new AES(Enumerable.Repeat((byte)0x00, 128 / 8).ToArray())) { }

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRF>(
				_prf,
				(F) =>
				{
					var c = F.PRF(_key, new byte[] { 0x00 });
					F.InversePRF(_key, c);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 2 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 2 }
				}
			);
		}
	}

	[Trait("Category", "Unit")]
	public class FeistelPRF : AbsPRF
	{
		public FeistelPRF() : base(new Feistel(3)) { }

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRF>(
				_prf,
				(F) =>
				{
					var c = F.PRF(_key, new byte[] { 0x00 });
					F.InversePRF(_key, c);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 6 },
					{ Primitive.PRP, 2 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRP, 2 }
				}
			);
		}
	}

	[Trait("Category", "Integration")]
	public class FeistelStrongPRF : AbsPRF
	{
		public FeistelStrongPRF() : base(new Feistel(4)) { }

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRF>(
				_prf,
				(F) =>
				{
					var c = F.PRF(_key, new byte[] { 0x00 });
					F.InversePRF(_key, c);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 8 },
					{ Primitive.PRP, 2 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRP, 2 }
				}
			);
		}
	}

	public abstract class AbsPRF
	{
		protected readonly IPRF _prf;
		private const int SEED = 123456;
		protected readonly byte[] _key = new byte[128 / 8];
		private const int RUNS = 1000;

		public AbsPRF(IPRF prf)
		{
			new Random(SEED).NextBytes(_key);
			_prf = prf;
		}

		[Fact]
		public void CorrectnessInt()
		{
			for (int i = -RUNS; i < RUNS; i++)
			{
				byte[] encrypted = _prf.PRF(_key, BitConverter.GetBytes(i));
				int decrypted = BitConverter.ToInt32(_prf.InversePRF(_key, encrypted), 0);
				Assert.Equal(i, decrypted);
			}
		}

		[Fact]
		public void Correctness()
		{
			Random random = new Random(SEED);

			for (int i = 0; i < RUNS; i++)
			{
				byte[] plaintext = new byte[4];
				random.NextBytes(plaintext);

				Assert.Equal(
					plaintext,
					_prf.InversePRF(
						_key,
						_prf.PRF(_key, plaintext)
					)
				);
			}
		}

		[Theory]
		[InlineData("Hello")]
		[InlineData("World")]
		[InlineData("")]
		[InlineData("1305")]
		public void StringCorrectness(string plaintext)
		{
			var ciphertext = _prf.PRF(_key, Encoding.Default.GetBytes(plaintext));

			var decrypted = _prf.InversePRF(_key, ciphertext);

			Assert.Equal(plaintext, Encoding.Default.GetString(decrypted));
		}
	}
}
