using System;
using System.Linq;
using ORESchemes.CJJKRS;

namespace Simulation.Protocol.SSE
{
	public class Server : AbsParty
	{
		private CJJKRS<Word, Index>.Server SSEServer;
		private readonly int _elementsPerPage;
		
		/// <param name="elementsPerPage">Number of TSet records fit per page (one record is 257 bits in this setting)</param>
		public Server(int elementsPerPage)
		{
			_elementsPerPage = elementsPerPage;
		}
		
		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case PublishDatabaseMessage database:
					SSEServer = new CJJKRS<Word, Index>.Server(database.Unpack());
					SSEServer.PageSize = _elementsPerPage * 257; // hardcoded, this number assumes CJJKRS SSE scheme
					return (IMessage<R>)new FinishMessage();
				case TokensMessage tokens:
					return (IMessage<R>)new EncryptedIndicesMessage(
						tokens.Unpack().Select(t => SSEServer.Search(t)).ToArray()
					);
			}
			
			throw new InvalidOperationException("Should not be here");
		}
	}
}
