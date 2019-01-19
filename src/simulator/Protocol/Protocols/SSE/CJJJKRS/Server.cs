using System;
using System.Linq;
using Crypto.CJJJKRS;

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Server : AbsParty
	{
		private Scheme<Word, Index>.Server SSEServer;
		private readonly int _elementsPerPage;

		private int _b;
		private int _B;

		public Server(int elementsPerPage)
		{
			_elementsPerPage = elementsPerPage;
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case PublishDatabaseMessage input:
					SSEServer = new Scheme<Word, Index>.Server(input.Unpack().db);
					SSEServer.PageSize = _elementsPerPage * (128 / 8); // AES block

					// register event handlers
					SSEServer.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
					SSEServer.NodeVisited += hash => OnNodeVisited(hash);
					SSEServer.OperationOcurred += operation => OnOperationOccurred(operation);

					// emulate database write to disk
					var pages = input.Unpack().db.Size / _elementsPerPage * (128 / 8);
					for (int i = 0; i < pages; i++)
					{
						OnNodeVisited(i);
					}
					
					_b = input.Unpack().b;
					_B = input.Unpack().B;

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
