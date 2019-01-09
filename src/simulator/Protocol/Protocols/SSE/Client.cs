using System.Collections.Generic;
using System.Linq;
using ORESchemes.CJJKRS;
using ORESchemes.Shared;
using static ORESchemes.CJJKRS.CJJKRS<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE
{
	public class Client : AbsClient
	{
		private readonly CJJKRS<Word, Index>.Client SSEClient;

		public Client(byte[] entropy)
		{
			SSEClient = new CJJKRS<Word, Index>.Client(entropy);

			SSEClient.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			SSEClient.NodeVisited += hash => OnNodeVisited(hash);
			SSEClient.OperationOcurred += operation => OnOperationOccurred(operation);
		}

		public override void RunConstruction(List<Record> input)
		{
			var pairs = input
				.DistinctBy(r => r.index)
				.Select(
					record =>
						Cover
							.Path(record.index.ToUInt())
							.Select(
								keyword => (
									index: new Index { Value = record.value },
									word: new Word { Value = keyword }
								)
							)
				)
				.SelectMany(l => l);

			var database = pairs
				.GroupBy(
					pair => pair.word,
					pair => pair.index
				)
				.ToDictionary(
					group => group.Key,
					group => group.ToArray()
				);

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
				Search(query.from, query.to);

				OnQueryCompleted();
			}
		}

		internal List<Index> Search(int from, int to)
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
