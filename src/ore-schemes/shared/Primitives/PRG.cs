using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ORESchemes.Shared.Primitives.PRG
{
	public class PRGFactory : AbsPrimitiveFactory<IPRG>
	{
		public PRGFactory(byte[] entropy = null) : base(entropy) { }

		protected override IPRG CreatePrimitive(byte[] entropy) => new AESPRG(entropy);
	}

	public class PRGCachedFactory : AbsPrimitiveFactory<IPRG>
	{
		public PRGCachedFactory(byte[] entropy = null) : base(entropy) { }

		protected override IPRG CreatePrimitive(byte[] entropy) => new AESPRGCached(entropy);
	}

	public class DefaultPRGFactory : AbsPrimitiveFactory<IPRG>
	{
		public DefaultPRGFactory(byte[] entropy = null) : base(entropy) { }

		protected override IPRG CreatePrimitive(byte[] entropy) => new DefaultRandom(entropy);
	}

	public interface IPRG : IPrimitive
	{
		/// <summary>
		/// Returns a 32-bits intger from its minimum value to its possible value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		int Next();

		/// <summary>
		/// Returns a 32-bits intger from 0 to the specified value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		int Next(int max);

		/// <summary>
		/// Returns a 32-bits intger withing the specified range
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		int Next(int min, int max);

		/// <summary>
		/// Returns a 64-bits intger from its minimum value to its possible value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		long NextLong();

		/// <summary>
		/// Returns a 64-bits intger from 0 to the specified value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		long NextLong(long max);

		/// <summary>
		/// Returns a 64-bits intger withing the specified range
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		long NextLong(long min, long max);

		/// <summary>
		/// Returns a double-precision floating-point number 
		/// from its minimum value to its possible value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		double NextDouble();

		/// <summary>
		/// Returns a double-precision floating-point number 
		/// from 0 to the specified value
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		double NextDouble(double max);

		/// <summary>
		/// Returns a double-precision floating-point number 
		/// withing the specified range
		/// inclusive sampled uniformly at (pseudo)random
		/// </summary>
		double NextDouble(double min, double max);

		/// <summary>
		/// Populates a supplied array with (pseudo)random bytes
		/// </summary>
		void NextBytes(byte[] bytes);
	}

	/// <summary>
	/// Generic PRG class to be derived by other PRGs
	/// </summary>
	public abstract class CustomPRG : System.Security.Cryptography.RandomNumberGenerator, IPRG
	{
		protected const int ALPHA = 128;
		protected readonly byte[] _seed = new byte[ALPHA / 8];

		public CustomPRG(byte[] seed)
		{
			if (seed.Length < ALPHA / 8)
			{
				var newArray = new byte[ALPHA / 8];

				var startAt = newArray.Length - seed.Length;
				Array.Copy(seed, 0, _seed, startAt, seed.Length);
			}
			else
			{
				Array.Copy(seed, 0, _seed, 0, _seed.Length);
			}
		}

		public event PrimitiveUsageEventHandler PrimitiveUsed;

		/// <summary>
		/// Emits the event that the primitive was used
		/// </summary>
		protected void OnUse(Primitive primitive, bool impure = false)
		{
			var handler = PrimitiveUsed;
			if (handler != null)
			{
				handler(primitive, impure);
			}
		}

		public int Next()
		{
			byte[] bytes = new byte[sizeof(int)];
			this.GetBytes(bytes);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToInt32(bytes, 0);
		}

		public int Next(int max) => Next(0, max);

		public int Next(int min, int max)
		{
			while (true)
			{
				uint large = unchecked((uint)this.Next());
				uint diff;
				if (min < 0)
				{
					diff = (uint)max + (uint)-min;
				}
				else
				{
					diff = (uint)max - (uint)min;
				}

				if (large > (uint.MaxValue / diff) * diff)
				{
					continue;
				}

				var result = (int)Math.Floor(min + ((double)large / UInt32.MaxValue) * (diff + 1));

				if (result > max || result < min)
				{
					continue;
				}

				return result;
			}
		}

		public void NextBytes(byte[] bytes) => GetBytes(bytes);

		public double NextDouble() => NextLong() * (1.0 / long.MaxValue);

		public double NextDouble(double max) => NextDouble(0, max);

		public double NextDouble(double min, double max)
		{
			while (true)
			{
				ulong large = unchecked((ulong)this.NextLong());
				double diff = max - min;

				var result = min + ((double)large / UInt64.MaxValue) * diff;

				if (result > max || result < min)
				{
					continue;
				}

				return result;
			}
		}

		public long NextLong()
		{
			byte[] bytes = new byte[sizeof(long)];
			this.GetBytes(bytes);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToInt64(bytes, 0);
		}

		public long NextLong(long max) => NextLong(0, max);

		public long NextLong(long min, long max)
		{
			while (true)
			{
				ulong large = unchecked((ulong)this.NextLong());
				ulong diff;
				if (min < 0)
				{
					diff = (ulong)max + (ulong)-min;
				}
				else
				{
					diff = (ulong)max - (ulong)min;
				}

				if (large > (ulong.MaxValue / diff) * diff)
				{
					continue;
				}

				var result = (long)Math.Floor(min + ((double)large / UInt64.MaxValue) * (diff + 1));

				if (result > max || result < min)
				{
					continue;
				}

				return result;
			}
		}
	}

	/// <summary>
	/// AES based PRG (CTR mode) with cache
	/// </summary>
	public class AESPRGCached : CustomPRG
	{
		const int BLOCK = 128;
		private const int CACHE = BLOCK / 8; // Cache one block of entropy
		private ulong _counter = 0;
		private int _position = 0;

		private byte[] _entropy = null;

		public AESPRGCached(byte[] seed) : base(seed) { }

		public override void GetBytes(byte[] data)
		{
			OnUse(Primitive.PRG);

			if (_entropy == null)
			{
				_entropy = new byte[CACHE];
				GenerateEntropy();
			}

			int dataPosition = 0;

			while (dataPosition < data.Length)
			{
				int fromCache = Math.Min(CACHE - _position, data.Length - dataPosition);

				Array.Copy(_entropy, _position, data, dataPosition, fromCache);

				dataPosition += fromCache;
				_position += fromCache;

				if (_position == CACHE)
				{
					GenerateEntropy();
					_position = 0;
				}
			}
		}

		/// <summary>
		/// Fills up cache with generated entropy
		/// </summary>
		private void GenerateEntropy()
		{
			OnUse(Primitive.AES, true);

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = ALPHA;
				aesAlg.Key = _seed;

				aesAlg.Mode = CipherMode.ECB;

				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption
				using (var msEncrypt = new MemoryStream())
				{
					using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						byte[] input = new byte[0];

						for (ulong i = _counter; i < _counter + CACHE; i++)
						{
							input = input.Concat(BitConverter.GetBytes(i)).Concat(new byte[(BLOCK / 8) - sizeof(ulong)]).ToArray();
						}

						csEncrypt.Write(input, 0, input.Length);
						csEncrypt.FlushFinalBlock();

						_entropy = msEncrypt.ToArray();
						_counter += CACHE;
					}
				}
			}
		}
	}

	/// <summary>
	/// AES based PRG (CTR mode)
	/// </summary>
	public class AESPRG : CustomPRG
	{
		private ulong _counter = 0;

		public AESPRG(byte[] seed) : base(seed) { }

		public override void GetBytes(byte[] data)
		{
			OnUse(Primitive.PRG);
			OnUse(Primitive.AES, true);

			byte[] encrypted;

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = _seed;

				aesAlg.Mode = CipherMode.ECB;

				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				for (int i = 0; i <= data.Length * 8 / aesAlg.BlockSize; i++)
				{
					// Create the streams used for encryption
					using (var msEncrypt = new MemoryStream())
					{
						using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
						{

							byte[] input = BitConverter.GetBytes(_counter);
							csEncrypt.Write(input, 0, input.Length);
							csEncrypt.FlushFinalBlock();

							encrypted = msEncrypt.ToArray();
							_counter++;
						}
					}

					var length = data.Length * 8 >= (i + 1) * aesAlg.BlockSize ? aesAlg.BlockSize : data.Length * 8 - i * aesAlg.BlockSize;
					Array.Copy(encrypted, 0, data, i * aesAlg.BlockSize / 8, length / 8);
				}
			}
		}
	}

	/// <summary>
	/// Built-in C# fast but insecure PRG
	/// </summary>
	public class DefaultRandom : CustomPRG
	{
		private Random G;

		public DefaultRandom(byte[] seed) : base(seed)
		{
			G = new Random(BitConverter.ToInt32(seed, 0));
		}

		public override void GetBytes(byte[] data)
		{
			OnUse(Primitive.PRG);

			G.NextBytes(data);
		}
	}
}
