using System;
using System.Linq;
using Crypto.CJJJKRS;

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Server : AbsParty
	{
		private Scheme<Word, Index>.Server SSEServer;
		private readonly int _elementsPerPage;

		private readonly int _b;
		private readonly int _B;

		public Server(int elementsPerPage, int b, int B)
		{
			_elementsPerPage = elementsPerPage;
			_b = b;
			_B = B;
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case PublishDatabaseMessage database:
					SSEServer = new Scheme<Word, Index>.Server(database.Unpack());
					SSEServer.PageSize = _elementsPerPage * (128 / 8); // AES block

					// register event handlers
					SSEServer.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
					SSEServer.NodeVisited += hash => OnNodeVisited(hash);
					SSEServer.OperationOcurred += operation => OnOperationOccurred(operation);

					// emulate database write to disk
					for (int i = 0; i < database.Unpack().Size / _elementsPerPage * (128 / 8); i++)
					{
						OnNodeVisited(i);
					}

					return (IMessage<R>)new FinishMessage();
				case TokensMessage tokens:
					return (IMessage<R>)new ResultMessage(
						tokens.Unpack().Select(t => SSEServer.Search(t, Index.FromBytes, _b, _B)).SelectMany(i => i).ToArray()
					);
			}

			throw new InvalidOperationException("Should not be here");
		}
	}
}
