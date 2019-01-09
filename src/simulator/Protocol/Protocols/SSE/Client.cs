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

			// foreach (var number in new List<int> { 36, 37, 39 })
			// {
			// 	System.Console.WriteLine($"{number} pairs: {pairs.Count(p => p.index.Value == number.ToString())}");
			// }

			// var database = new Dictionary<Word, Index[]>();
			// var uniqueKeywords = pairs.Select(p => p.keyword).Distinct();
			// System.Console.WriteLine($"unique keywords: {uniqueKeywords.Count()}");

			// foreach (var keyword in uniqueKeywords)
			// {
			// 	database.Add(
			// 		new Word { Value = keyword },
			// 		pairs.Where(p => p.keyword.Equals(keyword)).Select(p => new Index { Value = p.value }).ToArray());
			// }

			var database = pairs
				.GroupBy(
					pair => pair.word,
					pair => pair.index
				)
				.ToDictionary(
					group => group.Key,
					group => group.ToArray()
				);

			// var hashSets = new List<HashSet<Word>>();
			// var intersection = new HashSet<Word>();
			// var levelTwo = new List<Word>();
			// foreach (var number in new List<int> { 36, 37, 39 })
			// {
			// 	System.Console.WriteLine($"{number} keys: {database.Values.Count(p => p.Any(i => i.Value == number.ToString()))}");
			// 	var hashSet = new HashSet<Word>(database.Where(kvp => kvp.Value.Any(i => i.Value == number.ToString())).Select(kvp => kvp.Key));
			// 	System.Console.WriteLine($"{number} distinct keys: {hashSet.Count}");
			// 	if (intersection.Count == 0)
			// 	{
			// 		intersection.Union(hashSet);
			// 	}
			// 	else
			// 	{
			// 		intersection.Intersect(hashSet);
			// 	}
			// 	levelTwo.Add(hashSet.Single(w => w.Value.Item2 == 2));
			// }

			// System.Console.WriteLine($"Hashset intersection size: {intersection.Count}");
			// System.Console.WriteLine($"LevelTwo size: {levelTwo.Count}");

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
