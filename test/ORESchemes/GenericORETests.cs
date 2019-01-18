using System;
using Xunit;
using Crypto.Shared;
using System.Linq;
using System.Collections.Generic;
using Crypto.Shared.Primitives;

namespace Test.Crypto
{
	public abstract class GenericORE<C, K>
		where C : IGetSize
		where K : IGetSize
	{
		protected IOREScheme<C, K> _scheme;
		protected readonly int _runs = 100;

		protected const int SEED = 123456;
		protected readonly byte[] _entropy = new byte[128 / 8];

		protected Dictionary<SchemeOperation, Tuple<int, int>> _expectedEvents = null;

		public GenericORE(int runs = 100)
		{
			_runs = runs;
			new Random(SEED).NextBytes(_entropy);
			SetParameters();
			SetScheme();
		}

		protected abstract void SetScheme();

		protected virtual void SetParameters() { }

		/// <summary>
		/// Sometimes (e.g. FH-OPE) ciphertext needs contain some more information
		/// before comparsions
		/// </summary>
		protected virtual C ConfigureCiphertext(C cipher, K key) => cipher;

		[Fact]
		public virtual void KeyGen()
		{
			var keyOne = _scheme.KeyGen();
			var keyTwo = _scheme.KeyGen();

			Assert.NotEqual(keyOne, keyTwo);
		}

		[Fact]
		/// <summary>
		/// Decryption of encryption should be original plaintext for all
		/// valid keys and plaintexts
		/// </summary>
		public void Correctness()
		{
			var generator = new Random(456784);
			var key = _scheme.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				var plaintext = generator.Next(Int32.MaxValue);

				Assert.Equal(
					_scheme.Decrypt(
						_scheme.Encrypt(plaintext, key),
						key
					),
					plaintext
				);
			}
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(-1, -1)]
		[InlineData(-1, 1)]
		[InlineData(1, -1)]
		[InlineData(2, 1)]
		[InlineData(1, 2)]
		[InlineData(-2, -1)]
		[InlineData(-1, -2)]
		public virtual void OrderCorrectness(int plaintextOne, int plaintextTwo)
		{
			var key = _scheme.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				var pOne = plaintextOne * (i + 1);
				var pTwo = plaintextTwo * (i + 1);

				var ciphertextOne = _scheme.Encrypt(pOne, key);
				var ciphertextTwo = _scheme.Encrypt(pTwo, key);

				ciphertextOne = ConfigureCiphertext(ciphertextOne, key);
				ciphertextTwo = ConfigureCiphertext(ciphertextTwo, key);

				Assert.Equal(pOne > pTwo, _scheme.IsGreater(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne < pTwo, _scheme.IsLess(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne >= pTwo, _scheme.IsGreaterOrEqual(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne <= pTwo, _scheme.IsLessOrEqual(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne == pTwo, _scheme.IsEqual(ciphertextOne, ciphertextTwo));
			}
		}

		[Fact]
		public void Events()
		{
			var actual = new Dictionary<SchemeOperation, int>();
			Enum
				.GetValues(typeof(SchemeOperation))
				.OfType<SchemeOperation>()
				.ToList()
				.ForEach(val => actual.Add(val, 0));

			_scheme.OperationOcurred += new SchemeOperationEventHandler(op => actual[op]++);

			var key = _scheme.KeyGen();

			var ciphertexts =
				Enumerable
					.Range(1, 10)
					.Select(val => _scheme.Encrypt(val, key))
					.ToList();

			ciphertexts.Select(c => ConfigureCiphertext(c, key)).ToList();

			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsGreater(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsGreaterOrEqual(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsLess(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsLessOrEqual(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsEqual(first, second)).ToList();

			ciphertexts
				.Select(val => _scheme.Decrypt(val, key))
				.ToList();

			var expected = _expectedEvents ?? new Dictionary<SchemeOperation, Tuple<int, int>>
			{
				{ SchemeOperation.KeyGen, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Encrypt, new Tuple<int, int>(10, 20) },
				{ SchemeOperation.Decrypt, new Tuple<int, int>(10, 15) },
				{ SchemeOperation.Comparison, new Tuple<int, int>(9 * 5, 9 * 5 * 4) },
			};

			foreach (var op in expected.Keys)
			{
				Assert.InRange(actual[op], expected[op].Item1, expected[op].Item2);
			}
		}

		[Fact]
		public void MinMax()
		{
			var key = _scheme.KeyGen();

			Assert.Equal(
				int.MinValue,
				_scheme.Decrypt(_scheme.MinCiphertextValue<C, K>(key), key)
			);

			Assert.Equal(
				int.MaxValue,
				_scheme.Decrypt(_scheme.MaxCiphertextValue<C, K>(key), key)
			);

			new List<int> {
				int.MinValue,
				int.MinValue / 2,
				-1, 0, 1,
				int.MaxValue / 2,
				int.MaxValue
			}.ForEach(
					num =>
					{
						var min = _scheme.MinCiphertextValue<C, K>(key);
						var max = _scheme.MaxCiphertextValue<C, K>(key);
						var @this = _scheme.Encrypt(num, key);

						min = ConfigureCiphertext(min, key);
						max = ConfigureCiphertext(max, key);
						@this = ConfigureCiphertext(@this, key);

						Assert.True(_scheme.IsLessOrEqual(min, @this));
						Assert.True(_scheme.IsGreaterOrEqual(max, @this));
					}
				);
		}

		[Fact]
		public virtual void PrimitivesEvents()
		{
			var key = _scheme.KeyGen();

			Dictionary<Primitive, long> primitiveUsage = new Dictionary<Primitive, long>();
			Dictionary<Primitive, long> purePrimitiveUsage = new Dictionary<Primitive, long>();

			Enum
				.GetValues(typeof(Primitive))
				.OfType<Primitive>()
				.ToList()
				.ForEach(val =>
				{
					primitiveUsage.Add(val, 0);
					purePrimitiveUsage.Add(val, 0);
				});

			_scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(primitive, impure) =>
				{
					primitiveUsage[primitive]++;
					if (!impure)
					{
						purePrimitiveUsage[primitive]++;
					}
				}
			);

			_scheme.IsLess(
				ConfigureCiphertext(_scheme.Encrypt(10, key), key),
				ConfigureCiphertext(_scheme.Encrypt(20, key), key)
			);

			Assert.NotEqual(0, primitiveUsage.Values.Sum());
			Assert.NotEqual(0, purePrimitiveUsage.Values.Sum());
		}

		[Fact]
		public void KeySizecheck()
		{
			var key = _scheme.KeyGen();
			Assert.Equal(KeySize(), key.GetSize());
		}

		[Fact]
		public void CipherSizeCheck()
		{
			var key = _scheme.KeyGen();
			var cipher = _scheme.Encrypt(50, key);

			Assert.Equal(CipherSize(), cipher.GetSize());
		}

		public virtual int KeySize() => 128;
		public virtual int CipherSize() => 64;
	}
}
