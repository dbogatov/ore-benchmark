using System;
using System.IO;
using System.Security.Cryptography;

namespace ORESchemes.Shared.Primitives.PRF
{
	public class PRFFactory : AbsPrimitiveFactory<IPRF>
	{
		protected override IPRF CreatePrimitive(byte[] entropy)
		{
			return new AESPRF();
		}
	}

	public interface IPRF : IPrimitive
	{
		/// <summary>
		/// Computes the value of the pseudo random function
		/// </summary>
		/// <param name="key">The key componenet to function</param>
		/// <param name="input">The input value to function</param>
		/// <returns>The value of the function of its arguments</returns>
		byte[] PRF(byte[] key, byte[] input);

		/// <summary>
		/// Computes the value of the inverse of pseudo random function
		/// </summary>
		/// <param name="key">The key componenet to function</param>
		/// <param name="input">The input value to function</param>
		/// <returns>The value of the inverse of function of its arguments</returns>
		byte[] InversePRF(byte[] key, byte[] input);
	}

	public class AESPRF : AbsPrimitive, IPRF
	{
		private const int ALPHA = 128;

		// https://gist.github.com/mark-adams/87aa34da3a5ed48ed0c7
		public byte[] PRF(byte[] key, byte[] input)
		{
			OnUse(Primitive.PRF);
			OnUse(Primitive.AES, true);

			byte[] encrypted;
			byte[] IV;

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = ALPHA;
				aesAlg.Key = key;

				IV = new byte[ALPHA / 8];

				aesAlg.IV = IV;
				aesAlg.Mode = CipherMode.CBC;

				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption
				using (var msEncrypt = new MemoryStream())
				{
					using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						csEncrypt.Write(input, 0, input.Length);
						csEncrypt.FlushFinalBlock();
					}
					encrypted = msEncrypt.ToArray();
				}
			}

			var ciphertext = new byte[encrypted.Length];
			Array.Copy(encrypted, 0, ciphertext, 0, encrypted.Length);

			// Return the encrypted bytes from the memory stream.
			return ciphertext;
		}

		// https://gist.github.com/mark-adams/87aa34da3a5ed48ed0c7
		public byte[] InversePRF(byte[] key, byte[] input)
		{
			OnUse(Primitive.PRF);
			OnUse(Primitive.AES, true);

			// Declare the string used to hold the decrypted text
			byte[] plaintext = null;

			// Create an Aes object with the specified key and IV
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = ALPHA;
				aesAlg.Key = key;

				aesAlg.IV = new byte[ALPHA / 8];
				aesAlg.Mode = CipherMode.CBC;

				// Create a decrytor to perform the stream transform
				var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for decryption. 
				using (var msDecrypt = new MemoryStream(input))
				{
					using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						// https://github.com/myloveCc/NETCore.Encrypt/blob/master/src/NETCore.Encrypt/EncryptProvider.cs
						using (var tempMemory = new MemoryStream())
						{
							byte[] buffer = new byte[1024];
							int readBytes = 0;
							while ((readBytes = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
							{
								tempMemory.Write(buffer, 0, readBytes);
							}

							plaintext = tempMemory.ToArray();
						}
					}
				}
			}

			return plaintext;
		}
	}
}
