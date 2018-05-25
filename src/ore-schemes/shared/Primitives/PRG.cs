using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	public class PRGFactory
	{
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

	public interface IPRG
	{
		int Next();
		int Next(int max);
		int Next(int min, int max);

		long NextLong();
		long NextLong(long max);
		long NextLong(long min, long max);

		double NextDouble();
		double NextDouble(double max);
		double NextDouble(double min, double max);

		void NextBytes(byte[] bytes);
	}

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

	public class AESPRG : CustomPRG
	{
		private long _counter = 0;

		public AESPRG(byte[] seed) : base(seed) { }

		public override void GetBytes(byte[] data)
		{
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

	public class DefaultRandom : CustomPRG
	{
		private Random _generator;

		public DefaultRandom(byte[] seed) : base(seed)
		{
			_generator = new Random(BitConverter.ToInt32(seed, 0));
		}

		public override void GetBytes(byte[] data)
		{
			_generator.NextBytes(data);
		}
	}
}
