using Simulation.Protocol;
using Xunit;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Test.Simulators.Protocols
{
	[Trait("Category", "Unit")]
	public class AbsProtocolChecks
	{
		private class TestProtocol : AbsProtocol
		{
			public TestProtocol(Mediator mediator)
			{
				_client = new Mock<AbsClient>().Object;
				_server = new Mock<AbsParty>().Object;

				SetupProtocol(mediator);
			}
		}

		public enum Stages
		{
			Handshake, Construction, Search
		}

		[Theory]
		[InlineData(Stages.Handshake)]
		[InlineData(Stages.Construction)]
		[InlineData(Stages.Search)]
		public void TimerEvents(Stages stage)
		{
			bool[] timerEvents = new bool[2];
			int index = 0;

			Mock<Mediator> mediator = new Mock<Mediator>(new Mock<AbsClient>().Object, new Mock<AbsParty>().Object);
			AbsProtocol protocol = new TestProtocol(mediator.Object);

			protocol.Timer += stop =>
			{
				timerEvents[index] = stop;
				index++;
			};

			switch (stage)
			{
				case Stages.Handshake:
					protocol.RunHandshake();
					break;
				case Stages.Construction:
					protocol.RunConstructionProtocol(null);
					break;
				case Stages.Search:
					protocol.RunQueryProtocol(null);
					break;
			}

			Assert.Equal(new bool[] { false, true }, timerEvents);
		}

		[Fact]
		public void PropagatesEventsFromMediator()
		{
			var triggers = new Dictionary<Events, bool>();
			Enum.GetValues(typeof(Events)).Cast<Events>().ToList().ForEach(e => triggers.Add(e, false));

			Mock<Mediator> mediator = new Mock<Mediator>(new Mock<AbsClient>().Object, new Mock<AbsParty>().Object);
			AbsProtocol protocol = new TestProtocol(mediator.Object);

			protocol.ClientStorage += n => triggers[Events.ClientStorage] = true;
			protocol.OperationOcurred += n => triggers[Events.SchemeOperation] = true;
			protocol.MessageSent += n => triggers[Events.MessageSent] = true;
			protocol.NodeVisited += n => triggers[Events.NodeVisited] = true;
			protocol.PrimitiveUsed += (n, i) => triggers[Events.PrimitiveUsage] = true;
			protocol.Timer += n => triggers[Events.Timer] = true;
			protocol.QueryCompleted += () => triggers[Events.QueryCompleted] = true;

			mediator.Raise(c => c.ClientStorage += null, 0);
			mediator.Raise(c => c.OperationOcurred += null, 0);
			mediator.Raise(c => c.MessageSent += null, 0);
			mediator.Raise(c => c.NodeVisited += null, 0);
			mediator.Raise(c => c.PrimitiveUsed += null, 0, false);
			mediator.Raise(c => c.Timer += null, true);
			mediator.Raise(c => c.QueryCompleted += null);

			Assert.All(triggers.Values, v => Assert.True(v));
		}
	}
}
