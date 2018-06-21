namespace ORESchemes.Shared.Primitives
{
	public enum Primitive
	{
		PRF, PRG, Hash, LFPRF, PRP, HGSampler, UniformSampler, BinomialSampler, PPH, TreeTraversal
	}

	public delegate void PrimitiveUsageEventHandler(Primitive primitive, bool impure);

	public interface IPrimitive
	{
		/// <summary>
		/// Event signaling that some primitive has been used
		/// </summary>
		event PrimitiveUsageEventHandler PrimitiveUsed;
	}

	public abstract class AbsPrimitive : IPrimitive
	{
		public event PrimitiveUsageEventHandler PrimitiveUsed;

		/// <summary>
		/// Emits the event that the primitive was used
		/// </summary>
		/// <param name="primitive">Primitive that was used</param>
		/// <param name="impure">True, if primitive was used from within another primitive</param>
		protected void OnUse(Primitive primitive, bool impure = false)
		{
			var handler = PrimitiveUsed;
			if (handler != null)
			{
				handler(primitive, impure);
			}
		}
	}
}
