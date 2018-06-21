using System;
using System.IO;
using System.Security.Cryptography;

namespace ORESchemes.Shared.Primitives.PRG
{
	public class PRGFactory
	{
		/// <summary>
		/// Returns an initialized instance of AES PRG
		/// Accepts optional entropy
		/// </summary>
		public static IPRG GetPRG(byte[] seed = null)
		{
			if (seed != null)
			{
				return new AESPRG(seed);
			}
			else
			{
				seed = new byte[256 / 8];
				new Random().NextBytes(seed);
				return new AESPRG(seed);
			}
		}

		/// <summary>
		/// Returns an initialized instance of built-in C# PRG
		/// Accepts optional entropy
		/// </summary>
		public static IPRG GetDefaultPRG(byte[] seed = null)
		{
			if (seed != null)
			{
				return new DefaultRandom(seed);
			}
			else
			{
				seed = new byte[256 / 8];
				new Random().NextBytes(seed);
				return new DefaultRandom(seed);
			}
		}
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
		protected const int ALPHA = 256;
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

		public int Next(int max)
		{
			return this.Next(0, max);
		}

		public int Next(int min, int max)
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

			return (int)Math.Round(min + ((double)large / UInt32.MaxValue) * (diff + 1));
		}

		public void NextBytes(byte[] bytes)
		{
			GetBytes(bytes);
		}

		public double NextDouble()
		{
			byte[] bytes = new byte[sizeof(double)];
			this.GetBytes(bytes);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToDouble(bytes, 0);
		}

		public double NextDouble(double max)
		{
			return this.NextDouble(0, max);
		}

		public double NextDouble(double min, double max)
		{
			ulong large = unchecked((ulong)this.NextLong());
			double diff = max - min;

			return min + ((double)large / UInt64.MaxValue) * diff;
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

		public long NextLong(long max)
		{
			return this.NextLong(0, max);
		}

		public long NextLong(long min, long max)
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

			return (long)Math.Round(min + ((double)large / UInt64.MaxValue) * (diff + 1));
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

			byte[] encrypted;

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = ALPHA;
				aesAlg.Key = _seed;
				aesAlg.IV = new byte[128 / 8];

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
