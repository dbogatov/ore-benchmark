using System.Collections.Generic;
using System.Linq;
using ORESchemes.CJJJKRS;
using ORESchemes.Shared;
using static ORESchemes.CJJJKRS.CJJJKRS<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Client : AbsClient, ISSEClient
	{
		private readonly CJJJKRS<Word, Index>.Client SSEClient;
		private byte[] _key;

		private readonly int _b;
		private readonly int _B;

		public Client(byte[] entropy, int b, int B)
		{
			SSEClient = new CJJJKRS<Word, Index>.Client(entropy);

			SSEClient.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			SSEClient.NodeVisited += hash => OnNodeVisited(hash);
			SSEClient.OperationOcurred += operation => OnOperationOccurred(operation);

			_b = b;
			_B = B;
		}

		public override void RunConstruction(List<Record> input)
		{
			var database = Utility.InputToDatabase(input);

			(var encrypted, var key) = SSEClient.Setup(database, _b, _B);
			_key = key;

			OnClientStorage(encrypted.Size);

			_mediator.SendToServer<Database, object>(
				new PublishDatabaseMessage(encrypted)
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
