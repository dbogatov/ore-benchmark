using System;

namespace Crypto.Shared.Primitives
{
	public enum Primitive
	{
		AES, PRF, Symmetric, PRG, Hash, LFPRF, PRP, HGSampler, UniformSampler, BinomialSampler, PPH, TreeTraversal, ORAMPath, ORAMLevel, TSet
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

		/// <summary>
		/// Hooks up the event handler to proxy primitive events through this instance
		/// </summary>
		/// <param name="primitive">The child primitive to proxy from</param>
		protected void RegisterPrimitive(IPrimitive primitive)
		{
			primitive.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) => OnUse(prim, true)
			);
		}
	}

	public abstract class AbsPrimitiveFactory<P> where P : IPrimitive
	{
		private readonly byte[] _entropy = null;

		public AbsPrimitiveFactory(byte[] entropy = null)
		{
			if (entropy == null)
			{
				_entropy = new byte[128 / 8];
				new Random().NextBytes(_entropy);
			}
			else
			{
				_entropy = entropy;
			}
		}
		public P GetPrimitive() => CreatePrimitive(_entropy);

		/// <summary>
		/// Returns an initialized instance of a the primitive
		/// </summary>
		protected abstract P CreatePrimitive(byte[] entropy);
	}

	public interface IByteable
	{
		/// <summary>
		/// An object will be put through PRF, so byte representation is required
		/// </summary>
		byte[] ToBytes();
	}
}
