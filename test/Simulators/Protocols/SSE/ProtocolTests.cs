using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Protocol;
using Simulation.Protocol.SSE;
using Xunit;

namespace Test.Simulators.Protocols.SSE
{
	[Trait("Category", "Unit")]
	public class CJJKRSProtocol : AbsProtocol
	{
		public CJJKRSProtocol() : base(2, new Simulation.Protocol.SSE.CJJKRS.Protocol(new byte[] { 0x13 }, 10)) { }
	}

	[Trait("Category", "Unit")]
	public class CJJJKRSProtocolNoPack : AbsProtocol
	{
		public CJJJKRSProtocolNoPack() : base(2, new Simulation.Protocol.SSE.CJJJKRS.Protocol(new byte[] { 0x13 }, 10)) { }
	}

	[Trait("Category", "Unit")]
	public class CJJJKRSProtocolPack : AbsProtocol
	{
		public CJJJKRSProtocolPack() : base(2, new Simulation.Protocol.SSE.CJJJKRS.Protocol(new byte[] { 0x13 }, 10, 5, 20)) { }
	}

	[Trait("Category", "Unit")]
	public class AbsProtocol
	{
		private readonly int _runs;
		private readonly Random _random = new Random(123456);
		private readonly ISSEProtocol _protocol;

		internal AbsProtocol(int runs, ISSEProtocol protocol)
		{
			var entropy = new byte[128 / 8];
			_random.NextBytes(entropy);

			_protocol = protocol;

			_runs = runs;
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
			for (int i = 0; i < _runs; i++)
			{
				var input = Enumerable
					.Range(0, _runs * 10)
					.Select(_ =>
					{
						var value = _random.Next(-_runs * 5, _runs * 5);
						return new Simulation.Protocol.Record(value, value.ToString());
					})
					.ToList();

				var range = (int)Math.Floor(_runs * 10 * 0.03);

				var queries = Enumerable
					.Range(0, _runs / 2)
					.Select(_ =>
					{
						var value = _random.Next(-_runs * 5, _runs * 5 - range);
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
