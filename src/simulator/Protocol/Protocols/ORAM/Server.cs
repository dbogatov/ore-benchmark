using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.ORAM
{
	public class Server : AbsParty
	{
		private readonly IPRG G;
		private readonly int _z;
		private readonly int _elementsPerPage;

		public Server(byte[] entropy, int z, int elementsPerPage)
		{
			G = new PRGFactory(entropy).GetPrimitive();
			_z = z;
			_elementsPerPage = elementsPerPage;
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
			OnPrimitiveUsed(Primitive.ORAMLevel, false);
			
			var pagesAccessed = (_z + _elementsPerPage - 1) / _elementsPerPage;
			var nodes = message.Unpack().Item2;
			var pages = (_z + _elementsPerPage - 1) / _elementsPerPage;

			for (int i = 0; i < pagesAccessed; i++)
			{	
				OnNodeVisited(pages > 0 ? 0 : G.Next(0, pages));
			}

			return new FinishMessage();
		}
	}
}
