using System.Collections.Generic;
using System.Linq;
using Crypto.CJJKRS;
using Crypto.Shared;
using static Crypto.CJJKRS.Scheme<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE.CJJKRS
{
	public class Client : AbsClient, ISSEClient
	{
		private readonly Scheme<Word, Index>.Client SSEClient;

		public Client(byte[] entropy)
		{
			SSEClient = new Scheme<Word, Index>.Client(entropy);

			SSEClient.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			SSEClient.NodeVisited += hash => OnNodeVisited(hash);
			SSEClient.OperationOcurred += operation => OnOperationOccurred(operation);
		}

		public override void RunConstruction(List<Record> input)
		{
			var database = Utility.InputToDatabase(input);
			
			var encrypted = SSEClient.Setup(database);

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
					.Select(keyword => SSEClient.Trapdoor(keyword))
					.ToArray();

			var encryptedIndices = _mediator.SendToServer<Token[], EncryptedIndices[]>(
				new TokensMessage(tokens)
			).Unpack();

			var result = new List<Index>();
			for (int i = 0; i < keywords.Length; i++)
			{
				result.AddRange(SSEClient.Decrypt(encryptedIndices[i], keywords[i], Index.FromBytes));
			}

			return result;
		}
	}
}
