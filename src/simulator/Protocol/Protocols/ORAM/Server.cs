using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.ORAM
{
	public class Server : AbsParty
	{
		private readonly IPRG G;
		private readonly int _z;

		public Server(byte[] entropy, int z)
		{
			G = new PRGFactory(entropy).GetPrimitive();
			_z = z;
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case WriteBucketMessage write:
					AcceptMessage(write);
					break;
				case ReadBucketMessage read:
					AcceptMessage(read);
					break;
			}

			return (IMessage<R>)new FinishMessage();
		}

		private void AcceptMessage(BucketMessage operation)
		{
			var nodes = operation.Unpack().Item2 / _z;

			OnNodeVisited(G.Next(0, nodes > 0 ? nodes : 1));
		}
	}
}
