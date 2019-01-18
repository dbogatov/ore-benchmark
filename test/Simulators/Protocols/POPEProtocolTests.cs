using System;
using System.Linq;
using Simulation.Protocol;
using Crypto.Shared;
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

		private readonly int L = 60;

		public POPEProtocol()
		{
			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8), L);

			_mediator = new Mediator(_client, _server);

			_client.SetMediator(_mediator);
			_server.SetMediator(_mediator);
		}

		[Fact]
		public void ClientResponseCorrectnessSortedList()
		{
			var input = Enumerable
				.Range(1, DISTINCT)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a =>
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					return new
					{
						encrypted = new EncryptedRecord<Cipher>
						{
							cipher = _client.ExportEncryption()(new Value(a, nonce, Origin.None)),
							value = $"{a}-{nonce}"
						},
						original = new Value(a, nonce, Origin.None).OrderValue
					};
				})
				.ToList();

			_client.AcceptMessage<HashSet<Cipher>, object>(
				new SetListMessage(
					new HashSet<Cipher>(input.Select(a => a.encrypted.cipher))
				)
			);

			var result = _client.AcceptMessage<object, List<Cipher>>(
				new GetSortedListMessage()
			).Unpack();

			var decrypted = result.Select(_client.ExportDecryption()).ToList();
			var expected = input.Select(c => c.original).OrderBy(c => c).ToList();

			Assert.Equal(expected, decrypted);
		}

		[Fact]
		public void ClientResponseCorrectnessIndex()
		{
			var input = Enumerable
				.Range(1, DISTINCT)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.Concat(new List<int> { int.MaxValue })
				.ToList()
				.Shuffle(G)
				.Select(a =>
				{
					var nonce = G.Next(Int32.MaxValue / 4);
					return new
					{
						encrypted = new EncryptedRecord<Cipher>
						{
							cipher = _client.ExportEncryption()(new Value(a, nonce, Origin.None)),
							value = $"{a}-{nonce}"
						},
						original = new Value(a, nonce, Origin.None).OrderValue
					};
				})
				.ToList();

			_client.AcceptMessage<HashSet<Cipher>, object>(
				new SetListMessage(
					new HashSet<Cipher>(input.Select(a => a.encrypted.cipher))
				)
			);

			var sorted = input.OrderBy(c => c.original).ToList();

			for (int i = 0; i < RUNS; i++)
			{
				var insert = new Value(G.Next(1, DISTINCT), G.Next(0, int.MaxValue / 4), Origin.None);

				var result = _client.AcceptMessage<Cipher, int>(
					new IndexOfResultMessage(_client.ExportEncryption()(insert))
				).Unpack();

				int expected = -1;
				for (int j = 0; j < sorted.Count; j++)
				{
					if (sorted[j].original >= insert.OrderValue)
					{
						expected = j;
						break;
					}
				}

				Assert.Equal(expected, result);
			}

			var right = _client.AcceptMessage<Cipher, int>(
				new IndexOfResultMessage(_client.ExportEncryption()(new Value(DISTINCT + 100, G.Next(0, int.MaxValue / 4), Origin.None)))
			).Unpack();

			Assert.Equal(sorted.Count - 1, right);
		}

		[Fact]
		public void SearchCorrectness()
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

			for (int i = 0; i < RUNS; i++)
			{
				var from = G.Next(1, DISTINCT);
				var to = G.Next(1, DISTINCT);

				if (from > to)
				{
					var tmp = to;
					to = from;
					from = tmp;
				}

				var result = _server.AcceptMessage<Tuple<Cipher, Cipher>, List<string>>(
					new QueryMessage<Cipher>(
						new Tuple<Cipher, Cipher>(
							_client.ExportEncryption()(new Value(from, G.Next(0, int.MaxValue / 4), Origin.Left)),
							_client.ExportEncryption()(new Value(to, G.Next(0, int.MaxValue / 4), Origin.Right))
						)
					)
				)
				.Unpack()
				.OrderBy(c => int.Parse(c))
				.ToHashSet();

				var expected = Enumerable
					.Range(from, to - from)
					.Select(a => Enumerable.Repeat(a, DUPLICATES))
					.SelectMany(a => a)
					.OrderBy(c => c)
					.Select(c => c.ToString())
					.ToHashSet();

				var min = result.Min(c => int.Parse(c));
				var max = result.Max(c => int.Parse(c));

				Assert.Superset(expected, result);
			}
		}
	}
}
