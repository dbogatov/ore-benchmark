using System.Collections.Generic;
using System.Linq;
using Crypto.CJJJKRS;
using Crypto.Shared;
using static Crypto.CJJJKRS.Scheme<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Client : AbsClient, ISSEClient
	{
		private readonly Scheme<Word, Index>.Client SSEClient;
		private byte[] _key;
		private readonly int _elementsPerPage;

		public Client(byte[] entropy, int elementsPerPage)
		{
			SSEClient = new Scheme<Word, Index>.Client(entropy);

			SSEClient.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			SSEClient.NodeVisited += hash => OnNodeVisited(hash);
			SSEClient.OperationOcurred += operation => OnOperationOccurred(operation);

			_elementsPerPage = elementsPerPage;
		}

		public override void RunConstruction(List<Record> input)
		{
			var database = Utility.InputToDatabase(input);

			var b = _elementsPerPage / 2;
			var B = _elementsPerPage;

			(var encrypted, var key) = SSEClient.Setup(database, b, B);
			_key = key;

			OnClientStorage(encrypted.Size);

			_mediator.SendToServer<(Database, int, int), object>(
				new PublishDatabaseMessage((encrypted, b, B))
			);

			OnQueryCompleted();
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				((ISSEClient)this).Search(query.from, query.to);

				OnQueryCompleted();
			}
		}

		/// <summary>
		/// An extraction of search logic for ease of testing
		/// </summary>
		/// <param name="from">Left endpoint of a query</param>
		/// <param name="to">Right endpoint of a query</param>
		/// <returns>A list of indices corresponding to given range</returns>
		List<Index> ISSEClient.Search(int from, int to)
		{
			var keywords =
					Cover
						.BRC(from.ToUInt(), to.ToUInt())
						.Select(tuple => new Word { Value = tuple })
						.ToArray();

			var tokens =
				keywords
					.Select(keyword => SSEClient.Trapdoor(keyword, _key))
					.ToArray();

			var result = _mediator.SendToServer<Token[], Index[]>(
				new TokensMessage(tokens)
			).Unpack();

			return result.ToList();
		}
	}
}
