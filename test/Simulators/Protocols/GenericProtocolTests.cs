using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Protocol;
using Xunit;

namespace Test.Simulators.Protocols.Integration
{
	public abstract class AbsProtocol
	{
		protected IProtocol _protocol;

		protected readonly List<Simulation.Protocol.Record> _input;
		protected readonly List<RangeQuery> _queries;
		protected readonly Dictionary<Events, bool> _triggers;

		protected virtual HashSet<Events> ExpectedTriggers
		{
			get
			{
				return Enum.GetValues(typeof(Events)).Cast<Events>().ToHashSet();
			}
		}

		public AbsProtocol()
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

			_triggers = new Dictionary<Events, bool>();
		}

		protected void SetupHandlers()
		{
			Enum.GetValues(typeof(Events)).Cast<Events>().ToList().ForEach(e => _triggers.Add(e, false));

			_protocol.ClientStorage += n => _triggers[Events.ClientStorage] = true;
			_protocol.OperationOcurred += n => _triggers[Events.SchemeOperation] = true;
			_protocol.MessageSent += n => _triggers[Events.MessageSent] = true;
			_protocol.NodeVisited += n => _triggers[Events.NodeVisited] = true;
			_protocol.PrimitiveUsed += (n, i) => _triggers[Events.PrimitiveUsage] = true;
			_protocol.Timer += (n) => _triggers[Events.Timer] = true;
			_protocol.QueryCompleted += () => _triggers[Events.QueryCompleted] = true;
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

			CheckTriggers();
		}

		[Fact]
		public void Search()
		{
			_protocol.RunHandshake();
			_protocol.RunConstructionProtocol(_input);
			_protocol.RunQueryProtocol(_queries);

			CheckTriggers();
		}

		private void CheckTriggers() 
			=> Enum
				.GetValues(typeof(Events))
				.Cast<Events>()
				.ToList()
				.ForEach(
					e => Assert.True(!ExpectedTriggers.Contains(e) || _triggers[e])
				);
	}
}
