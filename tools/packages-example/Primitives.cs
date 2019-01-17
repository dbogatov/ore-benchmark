using System;
using System.Collections;
using System.Text;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.Hash;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.PRP;
using ORESchemes.Shared.Primitives.Sampler;
using ORESchemes.Shared.Primitives.Symmetric;

namespace Packages
{
	partial class Program
	{
		static private void Primitives()
		{
			// Generate key
			var _G = new PRGFactory(new byte[] { 13 }).GetPrimitive();
			var key = _G.GetBytes(128 / 8);

			// PRG
			void PRG(IPRG G)
			{
				// integers
				var number = G.Next();
				number = G.Next(3);
				number = G.Next(0, 3);

				// doubles
				var real = G.NextDouble();
				real = G.NextDouble(3);
				real = G.NextDouble(0, 3);

				// int64
				var big = G.NextLong();
				big = G.NextLong(3);
				big = G.NextLong(0, 3);

				// raw bytes
				var bytes = new byte[128 / 8];
				G.NextBytes(bytes); // fill
				bytes = G.GetBytes(128 / 8); // return
			}

			PRG(new PRGFactory(new byte[] { 13 }).GetPrimitive());
			PRG(new PRGCachedFactory(new byte[] { 13 }).GetPrimitive());

			// PRF
			var F = new PRFFactory().GetPrimitive();

			var encoded = F.PRF(key, Encoding.Default.GetBytes("Hello"));
			var original = F.InversePRF(key, encoded);
			// byte representation of "Hello"

			// Hash
			void Hash(IHash H)
			{
				var unkeyed = H.ComputeHash(Encoding.Default.GetBytes("Hello"));
				var keyed = H.ComputeHash(Encoding.Default.GetBytes("Hello"), key);
			}

			Hash(new HashFactory().GetPrimitive());
			Hash(new Hash512Factory().GetPrimitive());

			// Symmetric encryption
			var E = new SymmetricFactory().GetPrimitive();

			var encrypted = E.Encrypt(key, Encoding.Default.GetBytes("Hello"));
			var decrypted = E.Decrypt(key, encrypted);
			// byte representation of "Hello"

			// PRP
			void PRP(IPRP P)
			{
				var input = new BitArray(BitConverter.GetBytes(5));
				var permuted = P.PRP(input, key, 3);
				var unpermuted = P.InversePRP(permuted, key, 3);
			}

			PRP(new PRPFactory().GetPrimitive());
			PRP(new StrongPRPFactory().GetPrimitive());

			void SimplifiedPRP(ISimplifiedPRP P)
			{
				byte input = 5;
				var permuted = P.PRP(input, key, 3);
				var unpermuted = P.InversePRP(permuted, key, 3);
			}

			SimplifiedPRP(new TablePRPFactory().GetPrimitive());
			SimplifiedPRP(new NoInvPRPFactory().GetPrimitive());

			// Sampler
			var S = new SamplerFactory(_G).GetPrimitive(); // requires IPRG

			var hg = S.HyperGeometric(99, 10, 25);
			var uniform = S.Uniform(2, 6);
			var binomial = S.Binomial(40, 0.5);

			// Other primitives (LF-PRF and T-set) are too scheme-specific
			// Look up relevant schemes' code to see how they work
		}
	}
}
