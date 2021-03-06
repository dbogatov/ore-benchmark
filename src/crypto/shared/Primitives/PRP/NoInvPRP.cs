using System;
using Crypto.Shared.Primitives.PRG;

namespace Crypto.Shared.Primitives.PRP
{
	public class NoInvPRPFactory : AbsPrimitiveFactory<ISimplifiedPRP>
	{
		protected override ISimplifiedPRP CreatePrimitive(byte[] entropy) => new NoInvPRP();
	}

	public class NoInvPRP : AbsPrimitive, ISimplifiedPRP
	{
		public byte PRP(byte input, byte[] key, byte bits)
		{
			OnUse(Primitive.PRP);

			return GeneratePermutation(key, bits, inv: false, input);
		}

		public byte InversePRP(byte input, byte[] key, byte bits)
		{
			OnUse(Primitive.PRP);

			return GeneratePermutation(key, bits, inv: true, input);
		}

		// https://www.geeksforgeeks.org/shuffle-a-given-array/
		// https://en.wikipedia.org/wiki/Random_permutation
		/// <summary>
		/// Generate permutation and returns a value or inverse value
		/// </summary>
		/// <param name="key">Key to use as seed to PRG</param>
		/// <param name="bits">Number of bits to permute</param>
		/// <param name="inv">True if inverse is requested</param>
		/// <param name="value">Rarameter to permute function</param>
		/// <returns>Permutation of supplied value</returns>
		private byte GeneratePermutation(byte[] key, byte bits, bool inv, byte value)
		{
			if (bits < 1 || bits > 8)
			{
				throw new ArgumentException($"Simplified PRP works only within 1 to {sizeof(byte) * 8} bits.");
			}

			byte[] permutation = new byte[] {
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
				32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
				64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
				96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127,
				128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
				160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191,
				192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
				224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
			};

			int[] bitsToMax = new int[] { 0, 1, 3, 7, 15, 31, 63, 127, 255 };

			IPRG G = new PRGCachedFactory(key).GetPrimitive();
			RegisterPrimitive(G);

			int max = bitsToMax[bits];

			if (value > max)
			{
				throw new ArgumentException($"Invalid input {value} for bitness {bits}.");
			}

			// Start from the last element and
			// swap one by one. We don't need to
			// run for the first element 
			// that's why i > 0
			for (int i = max; i > byte.MinValue; i--)
			{
				// Pick a random index
				// from 0 to i
				int j = G.Next(0, i);

				// Swap arr[i] with the
				// element at random index
				byte temp = permutation[i];
				permutation[i] = permutation[j];
				permutation[j] = temp;
			}

			if (!inv)
			{
				return permutation[value];
			}
			else
			{
				return (byte)Array.IndexOf(permutation, value);
			}
		}
	}
}
