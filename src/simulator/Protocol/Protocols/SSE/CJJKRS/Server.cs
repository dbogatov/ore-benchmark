using System;
using System.Linq;
using Crypto.CJJKRS;

namespace Simulation.Protocol.SSE.CJJKRS
{
	public class Server : AbsParty
	{
		private Scheme<Word, Index>.Server SSEServer;
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
					SSEServer = new Scheme<Word, Index>.Server(database.Unpack());
					SSEServer.PageSize = _elementsPerPage * 257; // hardcoded, this number assumes CJJKRS SSE scheme

					// register event handlers
					SSEServer.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
					SSEServer.NodeVisited += hash => OnNodeVisited(hash);
					SSEServer.OperationOcurred += operation => OnOperationOccurred(operation);

					// emulate database write to disk
					for (int i = 0; i < database.Unpack().Size / (_elementsPerPage * 257); i++)
					{
						OnNodeVisited(i);
					}

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
