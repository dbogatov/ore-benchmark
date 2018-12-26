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
			=> (IMessage<R>)AcceptMessage((BucketMessage)message);

		private FinishMessage AcceptMessage(BucketMessage operation)
		{
			var nodes = operation.Unpack().Item2 / _z;

			OnNodeVisited(G.Next(0, nodes > 0 ? nodes : 1));
			
			return new FinishMessage();
		}
	}
}
