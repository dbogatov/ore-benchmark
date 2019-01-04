namespace Simulation.Protocol.SSE
{
	public class Server : AbsParty
	{
		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			throw new System.NotImplementedException();
		}
	}
}
