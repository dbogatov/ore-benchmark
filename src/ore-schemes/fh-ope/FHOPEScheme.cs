using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public class FHOPEScheme : AbsStatefulOPEScheme<State>
	{
		private long min;
		private long max;

		public FHOPEScheme(long min, long max, byte[] seed = null) : base(seed)
		{
			this.min = min;
			this.max = max;

			States = new Dictionary<byte[], State>();
		}

		public override int Decrypt(long ciphertext, byte[] key)
		{
			if (!States.ContainsKey(key))
			{
				throw new InvalidOperationException($"Scheme has never been used with the supplied key.");
			}

			return States[key].Get(ciphertext);
		}

		public override long Encrypt(int plaintext, byte[] key)
		{
			if (!States.ContainsKey(key))
			{
				byte[] entropy = new byte[256 / 8];
				G.NextBytes(entropy);
				IPRG prg = PRGFactory.GetPRG(entropy);

				States.Add(key, new State(prg, min, max));
			}

			return States[key].Insert(plaintext);
		}
	}
}
