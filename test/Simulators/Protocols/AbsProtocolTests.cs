using Simulation.Protocol;
using Xunit;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

namespace Test.Simulators.Protocols
{
	[Trait("Category", "Unit")]
	public class AbsProtocolTests
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

			mediator.Raise(c => c.ClientStorage += null, 0);
			mediator.Raise(c => c.OperationOcurred += null, 0);
			mediator.Raise(c => c.MessageSent += null, 0);
			mediator.Raise(c => c.NodeVisited += null, 0);
			mediator.Raise(c => c.PrimitiveUsed += null, 0, false);


			Assert.All(triggers.Values, v => Assert.True(v));
		}
	}
}
