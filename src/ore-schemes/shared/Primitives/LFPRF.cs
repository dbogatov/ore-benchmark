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
			base(new PRFFactory().GetPrimitive().PRF(key, entropy))
		{
			G = new PRGCachedFactory(_seed).GetPrimitive();

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
