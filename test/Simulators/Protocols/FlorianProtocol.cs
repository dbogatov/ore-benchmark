using System;
using System.Collections.Generic;
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

		public FlorianProtocol()
		{
			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8));

			var mediator = new Mediator(_client, _server);

			_client.SetMediator(mediator);
			_server.SetMediator(mediator);
		}

		[Fact]
		public void InsertionCorrectness()
		{
			var input = Enumerable
				.Range(1, 10)
				.Select(a => Enumerable.Repeat(a, 3))
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
					.Range(1, 10)
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
		}
	}
}
