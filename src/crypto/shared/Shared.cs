using Crypto.Shared.Primitives;
using Crypto.Shared.Primitives.PRG;
using Crypto.Shared.Primitives.Symmetric;

namespace Crypto.Shared
{
	public enum Protocols
	{
		NoEncryption,
		BCLO,
		CLWW,
		LewiWu,
		FHOPE,
		CLOZ,
		Kerschbaum,
		POPE,
		ORAM,
		CJJKRS,
		CJJJKRS
	}

	public enum SchemeOperation
	{
		KeyGen, Encrypt, Decrypt, Comparison
	}

	public delegate void NodeVisitedEventHandler(int nodeHash);
	public delegate void SchemeOperationEventHandler(SchemeOperation operation);

	public interface IGetSize
	{
		/// <summary>
		/// Returns the size of the entity in bits (not bytes)
		/// </summary>
		int GetSize();
	}

	/// <summary>
	/// A wrapper around int64 that supports IGetSize
	/// </summary>
	public class OPECipher : IGetSize
	{
		public long value;

		public OPECipher(long value) => this.value = value;
		public OPECipher() { }

		public int GetSize() => sizeof(long) * 8;

		public static bool operator <(OPECipher a, OPECipher b) => a.value < b.value;
		public static bool operator >(OPECipher a, OPECipher b) => a.value > b.value;
		public static bool operator <=(OPECipher a, OPECipher b) => a.value <= b.value;
		public static bool operator >=(OPECipher a, OPECipher b) => a.value >= b.value;
		public static bool operator ==(OPECipher a, OPECipher b) => a.value == b.value;
		public static bool operator !=(OPECipher a, OPECipher b) => a.value != b.value;

		public override int GetHashCode() => value.GetHashCode();
		public override bool Equals(object obj) => obj is OPECipher ? value == ((OPECipher)obj).value : false;

		public static OPECipher FromInt(int from) => new OPECipher { value = from };
		public int ToInt() => (int)value;

		public override string ToString() => value.ToString();

		public static implicit operator long(OPECipher c) => c.value;
		public static implicit operator OPECipher(long v) => new OPECipher(v);
	}

	/// <summary>
	/// A warpper around byte[] that supports IGetSize
	/// </summary>
	public class BytesKey : IGetSize
	{
		public byte[] value = new byte[] { };

		public BytesKey() { }
		public BytesKey(byte[] value) => this.value = value;

		public int GetSize() => value.Length * sizeof(byte) * 8;

		public static implicit operator byte[] (BytesKey k) => k.value;
		public static implicit operator BytesKey(byte[] v) => new BytesKey(v);
	}

	/// <summary>
	/// Base functionality for ORE operations
	/// </summary>
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
	/// Defines a encryption portion of ORE
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	/// <typeparam name="K">Key type</typeparam>
	public interface IOREEncryption<C, K> : IBaseORE
		where C : IGetSize
		where K : IGetSize
	{
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

	/// <summary>
	/// Defines a comparison portion of ORE
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
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
	}

	/// <summary>
	/// Defines a generic Order Preserving Encryption scheme
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	/// <typeparam name="K">Key type</typeparam>
	public interface IOREScheme<C, K> : IOREEncryption<C, K>, IOREComparator<C>
		where C : IGetSize
		where K : IGetSize
	{ }

	/// <summary>
	/// A class that encapsulate event handling functionality
	/// </summary>
	public abstract class EventHandlers
	{
		public event SchemeOperationEventHandler OperationOcurred;

		public event PrimitiveUsageEventHandler PrimitiveUsed;
		
		public event NodeVisitedEventHandler NodeVisited;

		/// <summary>
		/// Emits the event that the construction has made an I/O request
		/// </summary>
		/// <param name="hash">An identifier of I/O page (for caching puposes)</param>
		public void OnVisit(int hash)
		{
			var handler = NodeVisited;
			if (handler != null)
			{
				handler(hash);
			}
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
	}

	/// <summary>
	/// A default implementation of the interface
	/// To be derived by ORE schemes
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	public abstract class AbsOREScheme<C, K> : EventHandlers, IOREScheme<C, K>
		where C : IGetSize
		where K : IGetSize
	{
		protected readonly IPRG G;
		protected readonly ISymmetric E;
		protected const int ALPHA = 128;

		/// <summary>
		/// Entropy is required for the scheme
		/// </summary>
		public AbsOREScheme(byte[] seed)
		{
			G = new PRGCachedFactory(seed).GetPrimitive();
			E = new SymmetricFactory().GetPrimitive();

			SubscribePrimitive(G);
			SubscribePrimitive(E);
		}

		public abstract int Decrypt(C ciphertext, K key);

		public abstract C Encrypt(int plaintext, K key);

		public virtual bool IsEqual(C ciphertextOne, C ciphertextTwo)
			=>
				!IsLess(ciphertextOne, ciphertextTwo) &&
				!IsLess(ciphertextTwo, ciphertextOne);

		public virtual bool IsGreater(C ciphertextOne, C ciphertextTwo)
			=>
				!IsLess(ciphertextOne, ciphertextTwo) &&
				IsLess(ciphertextTwo, ciphertextOne);

		public virtual bool IsGreaterOrEqual(C ciphertextOne, C ciphertextTwo)
			=> !IsLess(ciphertextOne, ciphertextTwo);

		public virtual bool IsLess(C ciphertextOne, C ciphertextTwo)
			=> Compare(ciphertextOne, ciphertextTwo);

		public virtual bool IsLessOrEqual(C ciphertextOne, C ciphertextTwo)
			=> !IsGreater(ciphertextOne, ciphertextTwo);

		public abstract K KeyGen();

		protected abstract bool Compare(C ciphertextOne, C ciphertextTwo);
	}

	/// <summary>
	/// Generic implementation of OPE scheme
	/// To be derived by OPE schemes
	/// </summary>
	public abstract class AbsOPEScheme<K> : AbsOREScheme<OPECipher, K>
		where K : IGetSize
	{
		public AbsOPEScheme(byte[] seed) : base(seed) { }

		public override bool IsEqual(OPECipher ciphertextOne, OPECipher ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne == ciphertextTwo;
		}

		public override bool IsGreater(OPECipher ciphertextOne, OPECipher ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne > ciphertextTwo;
		}

		public override bool IsGreaterOrEqual(OPECipher ciphertextOne, OPECipher ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne >= ciphertextTwo;
		}

		public override bool IsLess(OPECipher ciphertextOne, OPECipher ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne < ciphertextTwo;
		}

		public override bool IsLessOrEqual(OPECipher ciphertextOne, OPECipher ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne <= ciphertextTwo;
		}

		protected override bool Compare(OPECipher ciphertextOne, OPECipher ciphertextTwo) => ciphertextOne.value < ciphertextTwo.value;
	}

	/// <summary>
	/// Generic implementation of ORE scheme that has CMP method
	/// that returns int (less, equal or greater)
	/// To be derived by ORE schemes
	/// </summary>
	public abstract class AbsORECmpScheme<C, K> : AbsOREScheme<C, K>
		where C : IGetSize
		where K : IGetSize
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
