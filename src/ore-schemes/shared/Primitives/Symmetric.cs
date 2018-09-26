using System;
using System.Security.Cryptography;

namespace ORESchemes.Shared.Primitives.Symmetric
{
	public class SymmetricFactory : AbsPrimitiveFactory<ISymmetric>
	{
		protected override ISymmetric CreatePrimitive(byte[] entropy) => new AESSymmetric();
	}

	public interface ISymmetric : IPrimitive
	{
		/// <summary>
		/// Symmetrically encrypts (randomized) a value
		/// </summary>
		/// <param name="key">The encryption key</param>
		/// <param name="input">The plaintext to encrypt</param>
		/// <returns>The randomized ciphertext for the input and key</returns>
		byte[] Encrypt(byte[] key, byte[] input);

		/// <summary>
		/// Symmetrically decrypts (deterministic) a value
		/// </summary>
		/// <param name="key">The encryption key</param>
		/// <param name="input">The ciphertext to decrypt</param>
		/// <returns>The determinstic plaintext for the input and key</returns>
		byte[] Decrypt(byte[] key, byte[] input);
	}

	public class AESSymmetric : AbsPrimitive, ISymmetric
	{
		private const int ALPHA = 128;

		// https://gist.github.com/magicsih/be06c2f60288b54d9f52856feb96ce8c
		public byte[] Encrypt(byte[] key, byte[] input)
		{
			OnUse(Primitive.Symmetric);
			OnUse(Primitive.AES, true);

			byte[] encrypted;
			byte[] IV;

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = key;

				aesAlg.GenerateIV();
				IV = aesAlg.IV;

				aesAlg.Mode = CipherMode.CBC;

				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);
			}

			var combinedIvCt = new byte[IV.Length + encrypted.Length];
			Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
			Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

			return combinedIvCt;
		}

		// https://gist.github.com/magicsih/be06c2f60288b54d9f52856feb96ce8c
		public byte[] Decrypt(byte[] key, byte[] input)
		{
			OnUse(Primitive.Symmetric);
			OnUse(Primitive.AES, true);

			// Create an Aes object with the specified key and IV
			using (Aes aesAlg = Aes.Create())
			{
				byte[] IV = new byte[aesAlg.BlockSize / 8];
				byte[] cipherText = new byte[input.Length - IV.Length];

				Array.Copy(input, IV, IV.Length);
				Array.Copy(input, IV.Length, cipherText, 0, cipherText.Length);

				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Key = key;
				aesAlg.IV = IV;

				ICryptoTransform transform = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
				byte[] decryptedValue = transform.TransformFinalBlock(cipherText, 0, cipherText.Length);

				return decryptedValue;
			}
		}
	}
}
