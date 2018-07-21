using System;
using System.Linq;
using Simulation.Protocol;
using ORESchemes.Shared;
using Xunit;
using Simulation.Protocol.POPE;

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
	}
}
