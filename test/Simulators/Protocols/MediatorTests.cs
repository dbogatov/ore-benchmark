using Simulation.Protocol;
using Xunit;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Test.Simulators.Protocols
{
	public enum Events
	{
		ClientStorage,
		SchemeOperation,
		NodeVisited,
		PrimitiveUsage,
		MessageSent
	}

	[Trait("Category", "Unit")]
	public class MediatorTests
	{
		private readonly Mock<AbsMessage<object>> _message = new Mock<AbsMessage<object>>();
		private readonly Mock<AbsMessage<object>> _response = new Mock<AbsMessage<object>>();

		private readonly Mock<AbsClient> _client = new Mock<AbsClient>();
		private readonly Mock<AbsParty> _server = new Mock<AbsParty>();
		private readonly Mediator _mediator;

		public MediatorTests()
		{
			_mediator = new Mediator(_client.Object, _server.Object);

			_message.Setup(m => m.GetSize()).Returns(10);
			_response.Setup(m => m.GetSize()).Returns(20);
		}

		[Fact]
		public void MessagePassedToClient()
		{
			SendToClient();

			_client.Verify(
				c => c.AcceptMessage<object, object>(
					It.IsAny<IMessage<object>>()
				)
			);
		}

		[Fact]
		public void MessagePassedToServer()
		{
			SendToServer();

			_server.Verify(
				c => c.AcceptMessage<object, object>(
					It.IsAny<IMessage<object>>()
				)
			);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void MessageSentEvent(bool sendToClient)
		{
			long messageSize = 0;
			_mediator.MessageSent += n => messageSize += n;

			if (sendToClient)
			{
				SendToClient();
			}
			else
			{
				SendToServer();
			}

			Assert.Equal(30, messageSize);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void RecordStorageEvent(bool sendToClient)
		{
			long clientStorage = 0;

			_client.CallBase = true;
			_client.Object.ClientStorage += n => clientStorage += n;

			if (sendToClient)
			{
				SendToClient();

				Assert.Equal(10, clientStorage);
			}
			else
			{
				SendToServer();

				Assert.Equal(20, clientStorage);
			}
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void PropagatesEvents(bool client)
		{
			var triggers = new Dictionary<Events, bool>();
			Enum.GetValues(typeof(Events)).Cast<Events>().ToList().ForEach(e => triggers.Add(e, false));

			_mediator.ClientStorage += n => triggers[Events.ClientStorage] = true;
			_mediator.OperationOcurred += n => triggers[Events.SchemeOperation] = true;
			_mediator.MessageSent += n => triggers[Events.MessageSent] = true;
			_mediator.NodeVisited += n => triggers[Events.NodeVisited] = true;
			_mediator.PrimitiveUsed += (n, i) => triggers[Events.PrimitiveUsage] = true;

			if (client)
			{
				_client.Raise(c => c.ClientStorage += null, 0);
				_client.Raise(c => c.OperationOcurred += null, 0);
				_client.Raise(c => c.MessageSent += null, 0);
				_client.Raise(c => c.NodeVisited += null, 0);
				_client.Raise(c => c.PrimitiveUsed += null, 0, false);
			}
			else
			{
				_server.Raise(c => c.ClientStorage += null, 0);
				_server.Raise(c => c.OperationOcurred += null, 0);
				_server.Raise(c => c.MessageSent += null, 0);
				_server.Raise(c => c.NodeVisited += null, 0);
				_server.Raise(c => c.PrimitiveUsed += null, 0, false);
			}

			Assert.All(triggers.Values, v => Assert.True(v));
		}

		private void SendToClient()
		{
			_client
				.Setup(
					s =>
						s.AcceptMessage<object, object>(
							It.IsAny<AbsMessage<object>>()
						)
				)
				.Returns(_response.Object);

			_mediator
				.SendToClient<object, object>(
					_message.Object
				);
		}

		private void SendToServer()
		{
			_server
				.Setup(
					s =>
						s.AcceptMessage<object, object>(
							It.IsAny<AbsMessage<object>>()
						)
				)
				.Returns(_response.Object);

			_mediator
				.SendToServer<object, object>(
					_message.Object
				);
		}
	}
}
