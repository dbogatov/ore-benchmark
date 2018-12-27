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

		/// <summary>
		/// React to a message from ORAM client.
		/// In this fake version of ORAM, just report I/Os that would have
		/// been made for a real PathORAM.
		/// </summary>
		/// <param name="message">A write or read buck message</param>
		/// <returns>A stub finish message</returns>
		private FinishMessage AcceptMessage(BucketMessage message)
		{
			var nodes = message.Unpack().Item2 / _z;

			OnNodeVisited(G.Next(0, nodes > 0 ? nodes : 1));
			
			return new FinishMessage();
		}
	}
}
