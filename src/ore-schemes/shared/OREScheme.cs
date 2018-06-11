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
		LewiORE
	}

	public enum SchemeOperation
	{
		Init, Destruct, KeyGen, Encrypt, Decrypt, Comparison
	}

	public delegate void SchemeOperationEventHandler(SchemeOperation operation);

	/// <summary>
	/// Defines a generic Order Preserving Encryption scheme
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	public interface IOREScheme<C>
	{
		/// <summary>
		/// Event signaling that some operation has ocurred
		/// </summary>
		event SchemeOperationEventHandler OperationOcurred;

		/// <summary>
		/// Event signaling that some primitive has been used
		/// </summary>
		event PrimitiveUsageEventHandler PrimitiveUsed;

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
		C Encrypt(int plaintext, byte[] key);

		/// <summary>
		/// Deterministic routine.
		/// decrypts given ciphertext using given key
		/// </summary>
		/// <param name="ciphertext">The ciphertext to decrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The plaintext of ciphertext using key</returns>
		int Decrypt(C ciphertext, byte[] key);

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
		/// Compares two values given by their ciphertexts
		/// </summary>
		/// <remark>
		/// This is a generic comparison method.
		/// Other comraisons will call this method.
		/// </remark>
		/// <param name="ciphertextOne">The first ciphertext to compare</param>
		/// <param name="ciphertextTwo">The second ciphertext to compare</param>
		/// <returns>True, if the first plaintext was less than the second, false otherwise</returns>
		bool Compare(C ciphertextOne, C ciphertextTwo);

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

	/// <summary>
	/// A default implementation of the interface
	/// To be derived by ORE schemes
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	public abstract class AbsOREScheme<C> : IOREScheme<C>
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

		public abstract int Decrypt(C ciphertext, byte[] key);

		public abstract C Encrypt(int plaintext, byte[] key);

		public virtual void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			PrimitiveUsed = null;

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

		public virtual byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[ALPHA / 8];
			G.NextBytes(key);

			maxCiphertextValue = Encrypt(MaxPlaintextValue(), key);
			minCiphertextValue = Encrypt(MinPlaintextValue(), key);

			_minMaxCiphertextsInitialized = true;

			return key;
		}

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
		protected void SubscribePrimitive(IPrimitive primitive)
		{
			primitive.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) =>
				{
					var handler = PrimitiveUsed;
					if (handler != null)
					{
						handler(prim, impure);
					}
				}
			);
		}

		public abstract bool Compare(C ciphertextOne, C ciphertextTwo);
	}

	/// <summary>
	/// Generic implementation of OPE scheme
	/// To be derived by OPE schemes
	/// </summary>
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

		sealed public override bool Compare(long ciphertextOne, long ciphertextTwo) => ciphertextOne < ciphertextTwo;
	}

	/// <summary>
	/// Generic implementation of ORE scheme that has CMP method
	/// that returns int (less, equal or greater)
	/// To be derived by ORE schemes
	/// </summary>
	public abstract class AbsORECmpScheme<C> : AbsOREScheme<C>
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

		sealed public override bool Compare(C ciphertextOne, C ciphertextTwo) => ProperCompare(ciphertextOne, ciphertextTwo) == -1;

		/// <summary>
		/// CMP as defined in https://eprint.iacr.org/2016/612.pdf Remark 2.3
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to compare</param>
		/// <param name="ciphertextTwo">Second ciphertext to compare</param>
		/// <returns>-1 if first < second, 0 if equal, 1 if first > second</returns>
		protected abstract int ProperCompare(C ciphertextOne, C ciphertextTwo);
	}

	public abstract class AbsStatefulOPEScheme<S> : AbsOPEScheme
	{
		public Dictionary<byte[], S> States { get; protected set; }

		public AbsStatefulOPEScheme(byte[] seed) : base(seed) { }
	}
}
