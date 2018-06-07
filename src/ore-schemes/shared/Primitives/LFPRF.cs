using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.Shared.Primitives.TapeGen
{
	/// <summary>
	/// TapeGen algorithms as in https://eprint.iacr.org/2012/624.pdf
	/// </summary>
	public class TapeGen : CustomPRG
	{
		private readonly IPRG G;

		private bool _used = false;

		public TapeGen(byte[] key, byte[] entropy) :
			base(PRFFactory.GetPRF().PRF(key, entropy, Enumerable.Repeat((byte)0x00, 128 / 8).ToArray()))
		{
			G = PRGFactory.GetPRG(_seed);

			G.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) => base.OnUse(prim, true)
			);
		}

		public override void GetBytes(byte[] data)
		{
			if (!_used)
			{
				base.OnUse(Primitive.PRF, true);
				_used = true;
			}

			OnUse(Primitive.LFPRF);

			G.NextBytes(data);
		}
	}
}
