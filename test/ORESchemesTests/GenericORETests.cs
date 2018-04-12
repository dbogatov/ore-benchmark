using System;
using Xunit;
using ORESchemes.Shared;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Test.ORESchemes
{
	public abstract class GenericORETests<C>
	{
		protected IOREScheme<int, C> _scheme;
		private readonly int _runs = 100;

		public GenericORETests()
		{
			SetScheme();
		}

		protected abstract void SetScheme();


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
		public void KeyGenTest()
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

			var generator = new Random();
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
		public void OrderCorrectnessTest(int plaintextOne, int plaintextTwo)
		{
			_scheme.Init();

			var key = _scheme.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				var pOne = plaintextOne * (i + 1);
				var pTwo = plaintextTwo * (i + 1);

				var ciphertextOne = _scheme.Encrypt(pOne, key);
				var ciphertextTwo = _scheme.Encrypt(pTwo, key);

				Assert.Equal(pOne > pTwo, _scheme.IsGreater(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne < pTwo, _scheme.IsLess(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne >= pTwo, _scheme.IsGreaterOrEqual(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne <= pTwo, _scheme.IsLessOrEqual(ciphertextOne, ciphertextTwo));
				Assert.Equal(pOne == pTwo, _scheme.IsEqual(ciphertextOne, ciphertextTwo));
			}
		}

		[Theory]
		[InlineData(SchemeOperation.Init)]
		[InlineData(SchemeOperation.KeyGen)]
		[InlineData(SchemeOperation.Destruct)]
		[InlineData(SchemeOperation.Encrypt)]
		[InlineData(SchemeOperation.Decrypt)]
		[InlineData(SchemeOperation.Comparison)]
		public void EventsTest(SchemeOperation operation)
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

			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsGreater(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsGreaterOrEqual(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsLess(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsLessOrEqual(first, second)).ToList();
			ciphertexts.Zip(ciphertexts.Skip(1), (first, second) => _scheme.IsEqual(first, second)).ToList();

			ciphertexts
				.Select(val => _scheme.Decrypt(val, key))
				.ToList();

			_scheme.Destruct();

			var expected = new Dictionary<SchemeOperation, Tuple<int, int>>
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
	}
}
