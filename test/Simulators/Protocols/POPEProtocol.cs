using System;
using System.Linq;
using Simulation.Protocol;
using ORESchemes.Shared;
using Xunit;
using Simulation.Protocol.POPE;
using System.Collections.Generic;

namespace Test.Simulators.Protocols
{
	[Trait("Category", "Unit")]
	public class POPEProtocol
	{
		private static int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly Client _client;
		private readonly Server _server;
		private readonly Mediator _mediator;

		private readonly int DISTINCT = 1000;
		private readonly int DUPLICATES = 5;
		private readonly int RUNS = 100;

		public POPEProtocol()
		{
			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8));

			_mediator = new Mediator(_client, _server);

			_client.SetMediator(_mediator);
			_server.SetMediator(_mediator);
		}

		[Fact]
		public void InsertionCorrectness()
		{
			var input = Enumerable
				.Range(1, DISTINCT)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a => new Simulation.Protocol.Record(a, a.ToString()))
				.ToList();

			_client.RunConstruction(input);

			Assert.True(
				_server.ValidateStructure(input.Select(c => c.index).ToList(), _client.ExportDecryption())
			);
		}

		[Fact]
		public void ClientResponseCorrectnessSortedList()
		{
			var input = Enumerable
				.Range(1, DISTINCT)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G);

			_client.AcceptMessage<HashSet<Cipher>, object>(
				new SetListMessage(
					new HashSet<Cipher>(input.Select(_client.ExportEncryption()))
				)
			);

			var result = _client.AcceptMessage<object, List<Cipher>>(
				new GetSortedListMessage()
			).Unpack();

			var decrypted = result.Select(_client.ExportDecryption()).ToList();
			var expected = input.OrderBy(c => c).ToList();

			Assert.Equal(expected, decrypted);
		}

		[Fact]
		public void ClientResponseCorrectnessIndex()
		{
			var input = Enumerable
				.Range(1, 100)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G);


			_client.AcceptMessage<HashSet<Cipher>, object>(
				new SetListMessage(
					new HashSet<Cipher>(input.Select(_client.ExportEncryption()).Concat(new List<Cipher> { null }))
				)
			);

			input.Add(int.MaxValue);

			var sorted = input.OrderBy(c => c).ToList();

			for (int i = 0; i < RUNS; i++)
			{
				var insert = G.Next(1, 100);

				var result = _client.AcceptMessage<Cipher, int>(
					new IndexOfResultMessage(_client.ExportEncryption()(insert))
				).Unpack();

				int expected = -1;
				for (int j = 0; j < sorted.Count; j++)
				{
					if (sorted[j] >= insert)
					{
						expected = j;
						break;
					}
				}

				Assert.Equal(expected, result);
			}

			var right = _client.AcceptMessage<Cipher, int>(
				new IndexOfResultMessage(_client.ExportEncryption()(150))
			).Unpack();

			Assert.Equal(sorted.Count - 1, right);
		}
	}
}
