using System;
using ORESchemes.Shared.Primitives;

namespace ORESchemes.Shared
{
	public enum ORESchemes
	{
		NoEncryption,
		CryptDB,
		PracticalORE
	}

	public enum SchemeOperation
	{
		Init, Destruct, KeyGen, Encrypt, Decrypt, Comparison
	}

	public delegate void SchemeOperationEventHandler(SchemeOperation operation);

	/// <summary>
	/// Defines a generic Order Preserving Encryption scheme
	/// T is a plaintex type, U is a ciphertext type
	/// </summary>
	public interface IOREScheme<P, C>
	{
		event SchemeOperationEventHandler OperationOcurred;

		/// <summary>
		/// Performs some work on initializing the scheme
		/// Eq. sets up some internal data, sample distributions, generates 
		/// internal keys
		/// </summary>
		void Init();

		/// <summary>
		/// Releases all resources created and managed by the scheme
		/// </summary>
		void Destruct();

		/// <summary>
		/// Randomized routine that generates a valid encryption key
		/// </summary>
		/// <returns>A valid encryption key</returns>
		byte[] KeyGen();

		/// <summary>
		/// Possibly randomized routine.
		/// Encrypts given plaintext using given key
		/// </summary>
		/// <param name="plaintext">The value to encrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The ciphertext of plaintext using key</returns>
		C Encrypt(P plaintext, byte[] key);

		/// <summary>
		/// Deterministic routine.
		/// decrypts given ciphertext using given key
		/// </summary>
		/// <param name="ciphertext">The ciphertext to decrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The plaintext of ciphertext using key</returns>
		P Decrypt(C ciphertext, byte[] key);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on equality
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>True if plaintexts were equal, and false otherwise</returns>
		bool IsEqual(C ciphertextOne, C ciphertextTwo);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on order
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>
		/// True if the first plaintext was greater than the second one, 
		/// and false otherwise
		/// </returns>
		bool IsGreater(C ciphertextOne, C ciphertextTwo);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on order
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>
		/// True if the first plaintext was less than the second one, 
		/// and false otherwise
		/// </returns>
		bool IsLess(C ciphertextOne, C ciphertextTwo);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on order
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>
		/// True if the first plaintext was greater than or equal to the second one, 
		/// and false otherwise
		/// </returns>
		bool IsGreaterOrEqual(C ciphertextOne, C ciphertextTwo);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on order
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>
		/// True if the first plaintext was less than or equal to the second one, 
		/// and false otherwise
		/// </returns>
		bool IsLessOrEqual(C ciphertextOne, C ciphertextTwo);

		/// <summary>
		/// Returns the encryption of the greatest possible value
		/// </summary>
		C MaxCiphertextValue();

		/// <summary>
		/// Returns the encryption of the smallest possible value
		/// </summary>
		C MinCiphertextValue();
	}

	public abstract class AbsOREScheme<C> : IOREScheme<int, C>
	{
		public event SchemeOperationEventHandler OperationOcurred;

		protected readonly IPRG _generator;
		protected const int ALPHA = 256;

		private C maxCiphertextValue = default(C);
		private C minCiphertextValue = default(C);

		public AbsOREScheme(byte[] seed)
		{
			_generator = PRGFactory.GetPRG(seed);
		}

		public abstract int Decrypt(C ciphertext, byte[] key);

		public abstract C Encrypt(int plaintext, byte[] key);

		public virtual void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			return;
		}
		public void Init()
		{
			OnOperation(SchemeOperation.Init);

			return;
		}

		public virtual bool IsEqual(C ciphertextOne, C ciphertextTwo)
		{
			return
				!IsLess(ciphertextOne, ciphertextTwo) &&
				!IsLess(ciphertextTwo, ciphertextOne);
		}

		public virtual bool IsGreater(C ciphertextOne, C ciphertextTwo)
		{
			return
				!IsLess(ciphertextOne, ciphertextTwo) &&
				!IsEqual(ciphertextOne, ciphertextTwo);
		}

		public virtual bool IsGreaterOrEqual(C ciphertextOne, C ciphertextTwo)
		{
			return !IsLess(ciphertextOne, ciphertextTwo);
		}

		public virtual bool IsLess(C ciphertextOne, C ciphertextTwo)
		{
			return Compare(ciphertextOne, ciphertextTwo);
		}

		public virtual bool IsLessOrEqual(C ciphertextOne, C ciphertextTwo)
		{
			return !IsGreater(ciphertextOne, ciphertextTwo);
		}

		public byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[ALPHA / 8];
			_generator.NextBytes(key);

			maxCiphertextValue = Encrypt(MaxPlaintextValue(), key);
			minCiphertextValue = Encrypt(MinPlaintextValue(), key);

			return key;
		}

		public virtual int MaxPlaintextValue() => Int32.MaxValue;
		public virtual int MinPlaintextValue() => Int32.MinValue;

		public C MaxCiphertextValue()
		{
			if (maxCiphertextValue == null)
			{
				throw new InvalidOperationException("Max value is generated during KeyGen operation");
			}

			return maxCiphertextValue;
		}

		public C MinCiphertextValue()
		{
			if (minCiphertextValue == null)
			{
				throw new InvalidOperationException("Min value is generated during KeyGen operation");
			}

			return minCiphertextValue;
		}

		/// <summary>
		/// Emits the event that scheme performed an operation
		/// </summary>
		/// <param name="operation">The operation that scheme performed</param>
		protected void OnOperation(SchemeOperation operation)
		{
			var handler = OperationOcurred;
			if (handler != null)
			{
				handler(operation);
			}
		}

		/// <summary>
		/// Compares two values given by their ciphertexts
		/// </summary>
		/// <param name="ciphertextOne">The first ciphertext to compare</param>
		/// <param name="ciphertextTwo">The second ciphertext to compare</param>
		/// <returns>True, if the first plaintext was less than the second, false otherwise</returns>
		protected abstract bool Compare(C ciphertextOne, C ciphertextTwo);
	}

	public abstract class AbsOPEScheme : AbsOREScheme<long>
	{
		public AbsOPEScheme(byte[] seed) : base(seed) { }

		public override bool IsEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne == ciphertextTwo;
		}

		public override bool IsGreater(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne > ciphertextTwo;
		}

		public override bool IsGreaterOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne >= ciphertextTwo;
		}

		public override bool IsLess(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne < ciphertextTwo;
		}

		public override bool IsLessOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne <= ciphertextTwo;
		}

		sealed protected override bool Compare(long ciphertextOne, long ciphertextTwo) => ciphertextOne < ciphertextTwo;
	}
}
