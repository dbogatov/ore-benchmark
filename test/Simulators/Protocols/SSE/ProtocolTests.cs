using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Protocol;
using Xunit;

namespace Test.Simulators.Protocols.SSE
{
	[Trait("Category", "Unit")]
	public class Protocol
	{
		private readonly int RUNS = 20;
		private readonly Random _random = new Random(123456);
		private readonly global::Simulation.Protocol.SSE.Protocol _protocol;

		public Protocol()
		{
			var entropy = new byte[128 / 8];
			_random.NextBytes(entropy);

			_protocol = new global::Simulation.Protocol.SSE.Protocol(entropy, 10);
		}

		[Fact]
		public void ConstructionNoExceptions()
			=> _protocol.RunConstructionProtocol(new List<Simulation.Protocol.Record> { new Simulation.Protocol.Record(5, 5.ToString()) });

		[Fact]
		public void SearchNoExceptions()
		{
			_protocol.RunConstructionProtocol(
				new List<Simulation.Protocol.Record> {
					new Simulation.Protocol.Record(5, 5.ToString()),
					new Simulation.Protocol.Record(7, 7.ToString())
				}
			);

			_protocol.RunQueryProtocol(new List<RangeQuery> { new RangeQuery(4, 6) });
		}

		[Fact]
		public void SearchTrivialCorrecness()
		{
			_protocol.RunConstructionProtocol(
				new List<Simulation.Protocol.Record> {
					new Simulation.Protocol.Record(5, 5.ToString()),
					new Simulation.Protocol.Record(7, 7.ToString())
				}
			);

			var result = _protocol.ExposeClient().Search(4, 6);

			var index = Assert.Single(result);
			Assert.Equal(5.ToString(), index.Value);
		}

		[Fact]
		public void SearchCorrecness()
		{
			for (int i = 0; i < RUNS; i++)
			{
				var input = Enumerable
					.Range(0, RUNS * 10)
					.Select(_ =>
					{
						var value = _random.Next(-RUNS * 5, RUNS * 5);
						return new Simulation.Protocol.Record(value, value.ToString());
					})
					.ToList();

				var range = (int)Math.Floor(RUNS * 10 * 0.03);

				var queries = Enumerable
					.Range(0, RUNS / 2)
					.Select(_ =>
					{
						var value = _random.Next(-RUNS * 5, RUNS * 5 - range);
						return new RangeQuery(value, value + range);
					})
					.ToList();

				_protocol.RunConstructionProtocol(input);

				var ordered = input.OrderBy(r => r.index);

				foreach (var query in queries)
				{
					var expected = ordered
						.SkipWhile(r => r.index < query.from)
						.TakeWhile(r => r.index <= query.to)
						.Select(r => r.value)
						.Distinct()
						.OrderBy(s => s);

					var actual = _protocol
						.ExposeClient()
						.Search(query.from, query.to)
						.Select(index => index.Value)
						.OrderBy(s => s);

					Assert.Equal(expected, actual);
				}
			}
		}
	}
}
