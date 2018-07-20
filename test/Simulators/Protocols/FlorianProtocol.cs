using System;
using System.Linq;
using Simulation.Protocol;
using ORESchemes.Shared;
using Xunit;
using Simulation.Protocol.Florian;

namespace Test.Simulators.Protocols
{
	public class FlorianProtocol
	{
		private static int SEED = 123456;
		private readonly Random G = new Random(SEED);
		private readonly Client _client;
		private readonly Server _server;
		private readonly Mediator _mediator;

		private readonly int DISTINCT = 1000;
		private readonly int DUPLICATES = 5;

		public FlorianProtocol()
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

			var queries = Enumerable
				.Range(1, 10)
				.Select(_ =>
				{
					int a = G.Next(1, 10);
					int b = G.Next(1, 10);

					return new RangeQuery(Math.Min(a, b), Math.Max(a, b));
				})
				.ToList();

			_client.RunConstruction(input);

			Assert.True(
				_server.ValidateStructure(_client.ExportDecryption())
			);
		}

		[Fact]
		public void QueryCorrectness()
		{
			var input = Enumerable
				.Range(1, DISTINCT)
				.Select(a => Enumerable.Repeat(a, DUPLICATES))
				.SelectMany(a => a)
				.ToList()
				.Shuffle(G)
				.Select(a => new Simulation.Protocol.Record(a, a.ToString()))
				.ToList();

			var queries = Enumerable
				.Range(1, DISTINCT)
				.Select(_ =>
				{
					int a = G.Next(1, DISTINCT);
					int b = G.Next(1, DISTINCT);

					return new RangeQuery(Math.Min(a, b), Math.Max(a, b));
				})
				.ToList();

			_client.RunConstruction(input);

			foreach (var query in queries)
			{
				var result = _client.Search(query);

				var expected = Enumerable
					.Range(query.from, query.to - query.from + 1)
					.Select(a => Enumerable.Repeat(a, DUPLICATES))
					.SelectMany(a => a)
					.OrderBy(a => a)
					.Select(a => a.ToString())
					.ToList();

				result = result.OrderBy(a => Int32.Parse(a)).ToList();

				Assert.Equal(expected, result);
			}
		}
	}
}
