using System;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.Shared
{
	public enum ORESchemes
	{
		NoEncryption,
		CryptDB,
		PracticalORE,
		LewiORE,
		FHOPE
	}

	public enum SchemeOperation
	{
		Init, Destruct, KeyGen, Encrypt, Decrypt, Comparison
	}

	public delegate void SchemeOperationEventHandler(SchemeOperation operation);

	public interface IBaseORE
	{
		/// <summary>
		/// Event signaling that some operation has ocurred
		/// </summary>
		event SchemeOperationEventHandler OperationOcurred;

		/// <summary>
		/// Event signaling that some primitive has been used
		/// </summary>
		event PrimitiveUsageEventHandler PrimitiveUsed;
	}

	/// <summary>
	/// Defines a generic Order Preserving Encryption scheme
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	public interface IOREEncryption<C, K> : IBaseORE
	{
		/// <summary>
		/// Performs some work on initializing the scheme
		/// Eq. sets up some internal data, sample distributions, generates 
		/// internal keys
		/// </summary>
		/// <returns>Self. Syntactic sugar to allow chaining.</returns>
		void Init();

		/// <summary>
		/// Releases all resources created and managed by the scheme
		/// </summary>
		void Destruct();

		/// <summary>
		/// Randomized routine that generates a valid encryption key
		/// </summary>
		/// <returns>A valid encryption key</returns>
		K KeyGen();

		/// <summary>
		/// Possibly randomized routine.
		/// Encrypts given plaintext using given key
		/// </summary>
		/// <param name="plaintext">The value to encrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The ciphertext of plaintext using key</returns>
		C Encrypt(int plaintext, K key);

		/// <summary>
		/// Deterministic routine.
		/// decrypts given ciphertext using given key
		/// </summary>
		/// <param name="ciphertext">The ciphertext to decrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The plaintext of ciphertext using key</returns>
		int Decrypt(C ciphertext, K key);
	}

	public interface IOREComparator<C> : IBaseORE
	{
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

		/// <summary>
		/// Returns the largest permissible plaintext value for the scheme
		/// </summary>
		int MaxPlaintextValue();

		/// <summary>
		/// Returns the smallest permissible plaintext value for the scheme
		/// </summary>
		int MinPlaintextValue();
	}

	public interface IOREScheme<C, K> : IOREEncryption<C, K>, IOREComparator<C> { }

	/// <summary>
	/// A default implementation of the interface
	/// To be derived by ORE schemes
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	public abstract class AbsOREScheme<C, K> : IOREScheme<C, K>
	{
		public event SchemeOperationEventHandler OperationOcurred;

		public event PrimitiveUsageEventHandler PrimitiveUsed;

		protected readonly IPRG G;
		protected const int ALPHA = 256;

		protected C maxCiphertextValue = default(C);
		protected C minCiphertextValue = default(C);

		protected bool _minMaxCiphertextsInitialized = false;

		/// <summary>
		/// Entropy is required for the scheme
		/// </summary>
		public AbsOREScheme(byte[] seed)
		{
			G = PRGFactory.GetPRG(seed);

			SubscribePrimitive(G);
		}

		public abstract int Decrypt(C ciphertext, K key);

		public abstract C Encrypt(int plaintext, K key);

		public virtual void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			PrimitiveUsed = null;
			OperationOcurred = null;

			return;
		}
		public virtual void Init()
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
				IsLess(ciphertextTwo, ciphertextOne);
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

		public abstract K KeyGen();

		public virtual int MaxPlaintextValue() => Int32.MaxValue;
		public virtual int MinPlaintextValue() => Int32.MinValue;

		public C MaxCiphertextValue()
		{
			if (!_minMaxCiphertextsInitialized)
			{
				throw new InvalidOperationException("Max ciphertext value is generated during KeyGen operation");
			}

			return maxCiphertextValue;
		}

		public C MinCiphertextValue()
		{
			if (!_minMaxCiphertextsInitialized)
			{
				throw new InvalidOperationException("Min ciphertext value is generated during KeyGen operation");
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
		/// Subscribe schemes primitive usage event to primitive's usage event.
		/// This way the delegate set to listen for scheme's event will be called
		/// each time primitive event fires up.
		/// </summary>
		/// <param name="primitive">Primitive which to subscribe</param>
		protected void SubscribePrimitive(IPrimitive primitive) =>
			primitive.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitive);

		/// <summary>
		/// Emits the event that some primitive was used
		/// </summary>
		/// <param name="prim">Primitive that was used</param>
		/// <param name="impure">True, if primitive was used from within another primitive</param>
		protected void OnPrimitive(Primitive prim, bool impure = false)
		{
			var handler = PrimitiveUsed;
			if (handler != null)
			{
				handler(prim, impure);
			}
		}

		protected abstract bool Compare(C ciphertextOne, C ciphertextTwo);
	}

	/// <summary>
	/// Generic implementation of OPE scheme
	/// To be derived by OPE schemes
	/// </summary>
	public abstract class AbsOPEScheme<K> : AbsOREScheme<long, K>
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

		protected override bool Compare(long ciphertextOne, long ciphertextTwo) => ciphertextOne < ciphertextTwo;
	}

	/// <summary>
	/// Generic implementation of ORE scheme that has CMP method
	/// that returns int (less, equal or greater)
	/// To be derived by ORE schemes
	/// </summary>
	public abstract class AbsORECmpScheme<C, K> : AbsOREScheme<C, K>
	{
		public AbsORECmpScheme(byte[] seed) : base(seed) { }

		public override bool IsEqual(C ciphertextOne, C ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ProperCompare(ciphertextOne, ciphertextTwo) == 0;
		}

		public override bool IsGreater(C ciphertextOne, C ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ProperCompare(ciphertextOne, ciphertextTwo) == 1;
		}

		public override bool IsGreaterOrEqual(C ciphertextOne, C ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ProperCompare(ciphertextOne, ciphertextTwo) != -1;
		}

		public override bool IsLess(C ciphertextOne, C ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ProperCompare(ciphertextOne, ciphertextTwo) == -1;
		}

		public override bool IsLessOrEqual(C ciphertextOne, C ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ProperCompare(ciphertextOne, ciphertextTwo) != 1;
		}

		sealed protected override bool Compare(C ciphertextOne, C ciphertextTwo) => ProperCompare(ciphertextOne, ciphertextTwo) == -1;

		/// <summary>
		/// CMP as defined in https://eprint.iacr.org/2016/612.pdf Remark 2.3
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to compare</param>
		/// <param name="ciphertextTwo">Second ciphertext to compare</param>
		/// <returns>-1 if first < second, 0 if equal, 1 if first > second</returns>
		protected abstract int ProperCompare(C ciphertextOne, C ciphertextTwo);
	}
}
