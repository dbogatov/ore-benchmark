using System;
using System.Collections;
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
		}

		public override void RunConstruction(List<Record> input)
		{
			var pairs = input
				.Select(
					record =>
						Cover.Path(record.index.ToUInt())
						.Select(keyword => (record.value, keyword))
				)
				.SelectMany(l => l);

			var database = pairs
				.GroupBy(
					pair => pair.keyword,
					pair => pair.value
				)
				.ToDictionary(
					group => new Word { Value = group.Key },
					group => group.Select(i => new Index { Value = i }).ToArray()
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
				var keywords =
					Cover
						.BRC(query.from.ToUInt(), query.to.ToUInt())
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
				// results list contains the answer

				OnQueryCompleted();
			}
		}
	}
}
