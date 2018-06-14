using System;
using Xunit;
using ORESchemes.Shared;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;

namespace Test.ORESchemes
{
	public abstract class GenericORETests<C, K>
	{
		protected IOREScheme<C, K> _scheme;
		protected readonly int _runs = 100;

		protected const int SEED = 123456;
		protected readonly byte[] _entropy = new byte[256 / 8];

		protected Dictionary<SchemeOperation, Tuple<int, int>> _expectedEvents = null;

		public GenericORETests(int runs = 100)
		{
			_runs = runs;
			new Random(SEED).NextBytes(_entropy);
			SetParameters();
			SetScheme();
		}

		~GenericORETests()
		{
			_scheme.Destruct();
		}

		protected abstract void SetScheme();

		protected virtual void SetParameters() { }

		protected virtual C ConfigureCiphertext(C cipher, K key) => cipher;

		[Fact]
		public void InitTest()
		{
			_scheme.Init();
		}

		[Fact]
		public void DestructTest()
		{
			_scheme.Init();
			_scheme.Destruct();
		}

		[Fact]
		public virtual void KeyGenTest()
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
		public void CorrectnessTest()
		{
			_scheme.Init();

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
		public virtual void OrderCorrectnessTest(int plaintextOne, int plaintextTwo)
		{
			_scheme.Init();

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
		public void EventsTest()
		{
			var actual = new Dictionary<SchemeOperation, int>();
			Enum
				.GetValues(typeof(SchemeOperation))
				.OfType<SchemeOperation>()
				.ToList()
				.ForEach(val => actual.Add(val, 0));

			_scheme.OperationOcurred += new SchemeOperationEventHandler(op => actual[op]++);

			_scheme.Init();
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

			_scheme.Destruct();

			var expected = _expectedEvents ?? new Dictionary<SchemeOperation, Tuple<int, int>>
			{
				{ SchemeOperation.Init, new Tuple<int, int>(1, 1)} ,
				{ SchemeOperation.KeyGen, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Destruct, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Encrypt, new Tuple<int, int>(10, 15) },
				{ SchemeOperation.Decrypt, new Tuple<int, int>(10, 15) },
				{ SchemeOperation.Comparison, new Tuple<int, int>(9 * 5, 9 * 5 * 4) },
			};

			foreach (var op in expected.Keys)
			{
				Assert.InRange(actual[op], expected[op].Item1, expected[op].Item2);
			}
		}

		[Fact]
		public void MinMaxTest()
		{
			_scheme.Init();
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
						Assert.True(
							_scheme.IsLessOrEqual(
								ConfigureCiphertext(_scheme.MinCiphertextValue<C, K>(key), key),
								ConfigureCiphertext(_scheme.Encrypt(num, key), key))
						);
						Assert.True(
							_scheme.IsGreaterOrEqual(
								ConfigureCiphertext(_scheme.MaxCiphertextValue<C, K>(key), key),
								ConfigureCiphertext(_scheme.Encrypt(num, key), key))
						);
					}
				);
		}

		[Fact]
		public virtual void PrimitivesEventsTest()
		{
			_scheme.Init();
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
	}
}
