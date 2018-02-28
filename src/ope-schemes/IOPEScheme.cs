using System;

namespace OPESchemes
{
	/// <summary>
	/// Defines a generic Order Preserving Encryption scheme
	/// </summary>
	public interface IOPEScheme
	{
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
		string KeyGen();

		/// <summary>
		/// Possibly randomized routine.
		/// Encrypts given plaintext using given key
		/// </summary>
		/// <param name="plaintext">The value to encrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The ciphertext of plaintext using key</returns>
		string Encrypt(string plaintext, string key);

		/// <summary>
		/// Deterministic routine.
		/// decrypts given ciphertext using given key
		/// </summary>
		/// <param name="ciphertext">The ciphertext to decrypt</param>
		/// <param name="key">The key to use in encryption</param>
		/// <returns>The plaintext of ciphertext using key</returns>
		string Decrypt(string ciphertext, string key);

		/// <summary>
		/// Deterministic routine.
		/// Tests two plaintexts given their encryptions produced with the same 
		/// key on equality
		/// </summary>
		/// <param name="ciphertextOne">First ciphertext to test</param>
		/// <param name="ciphertextTwo">Second ciphertext to test</param>
		/// <returns>True if plaintexts were equal, and false otherwise</returns>
		bool IsEqual(string ciphertextOne, string ciphertextTwo);

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
		bool IsGreater(string ciphertextOne, string ciphertextTwo);
	}
}
