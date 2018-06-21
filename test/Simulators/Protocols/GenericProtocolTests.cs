using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Protocol;
using Xunit;

namespace Test.Simulators.Protocols.Integration
{
	public abstract class AbsProtocolTests
	{
		protected IProtocol _protocol;

		protected readonly List<Simulation.Protocol.Record> _input;
		protected readonly List<RangeQuery> _queries;

		public AbsProtocolTests()
		{
			Random random = new Random(123456);

			_input = Enumerable
				.Range(1, 20)
				.OrderBy(n => random.Next())
				.Select(n => new Simulation.Protocol.Record(n, ""))
				.ToList();

			_queries = Enumerable
				.Range(1, 20)
				.Select(n => new RangeQuery(random.Next(1, 10), random.Next(11, 20)))
				.ToList();
		}

		[Fact]
		public void Handshake()
		{
			_protocol.RunHandshake();
		}

		[Fact]
		public void Construction()
		{
			_protocol.RunHandshake();
			_protocol.RunConstructionProtocol(_input);
		}

		[Fact]
		public void Search()
		{
			_protocol.RunHandshake();
			_protocol.RunConstructionProtocol(_input);
			_protocol.RunQueryProtocol(_queries);
		}
	}
}
