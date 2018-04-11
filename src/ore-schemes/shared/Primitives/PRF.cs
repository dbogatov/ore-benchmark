using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	public class PRFFactory
	{
		public static IPRF GetPRF()
		{
			return new AES();
		}
	}

	public interface IPRF
	{
		byte[] PRF(byte[] key, byte[] input);
		byte[] InversePRF(byte[] key, byte[] input);
		void SetSecurityParameter(int alpha);
	}

	public class AES : IPRF
	{
		private int _alpha = 256;

		public void SetSecurityParameter(int alpha)
		{
			_alpha = alpha;
		}

		// https://gist.github.com/mark-adams/87aa34da3a5ed48ed0c7
		public byte[] PRF(byte[] key, byte[] input)
		{
			byte[] encrypted;
			byte[] IV;

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = _alpha;
				aesAlg.Key = key;

				aesAlg.GenerateIV();
				IV = aesAlg.IV;

				aesAlg.Mode = CipherMode.CBC;

				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption. 
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

			var combinedIvCt = new byte[IV.Length + encrypted.Length];
			Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
			Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

			// Return the encrypted bytes from the memory stream. 
			return combinedIvCt;
		}

		// https://gist.github.com/mark-adams/87aa34da3a5ed48ed0c7
		public byte[] InversePRF(byte[] key, byte[] input)
		{
			// Declare the string used to hold 
			// the decrypted text. 
			byte[] plaintext = null;

			// Create an Aes object 
			// with the specified key and IV. 
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.KeySize = _alpha;
				aesAlg.Key = key;

				byte[] IV = new byte[aesAlg.BlockSize / 8];
				byte[] cipherText = new byte[input.Length - IV.Length];

				Array.Copy(input, IV, IV.Length);
				Array.Copy(input, IV.Length, cipherText, 0, cipherText.Length);

				aesAlg.IV = IV;
				aesAlg.Mode = CipherMode.CBC;

				// Create a decrytor to perform the stream transform.
				var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for decryption. 
				using (var msDecrypt = new MemoryStream(cipherText))
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
