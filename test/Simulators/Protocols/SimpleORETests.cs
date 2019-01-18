using System;
using System.Collections.Generic;
using System.Linq;
using BPlusTree;
using Moq;
using Moq.Protected;
using Crypto.Shared;
using Crypto.Shared.Primitives;
using Simulation.Protocol;
using Simulation.Protocol.SimpleORE;
using Xunit;

namespace Test.Simulators.Protocols.SimpleORE
{
	public static class ProtocolHelper
	{
		public static Mock<IOREScheme<OPECipher, BytesKey>> GetScheme()
		{
			var scheme = new Mock<IOREScheme<OPECipher, BytesKey>>();

			scheme
				.Setup(s => s.Encrypt(It.IsAny<int>(), It.IsAny<BytesKey>()))
				.Returns<int, BytesKey>((p, k) => new OPECipher(p));

			scheme
				.Setup(s => s.Decrypt(It.IsAny<OPECipher>(), It.IsAny<BytesKey>()))
				.Returns<OPECipher, BytesKey>((p, k) => (int)p.value);

			scheme
				.Setup(s => s.KeyGen())
				.Returns(new BytesKey());

			scheme
				.Setup(s => s.IsEqual(It.IsAny<OPECipher>(), It.IsAny<OPECipher>()))
				.Returns<OPECipher, OPECipher>((c, d) => c == d);

			scheme
				.Setup(s => s.IsGreater(It.IsAny<OPECipher>(), It.IsAny<OPECipher>()))
				.Returns<OPECipher, OPECipher>((c, d) => c > d);

			scheme
				.Setup(s => s.IsLess(It.IsAny<OPECipher>(), It.IsAny<OPECipher>()))
				.Returns<OPECipher, OPECipher>((c, d) => c < d);

			scheme
				.Setup(s => s.IsGreaterOrEqual(It.IsAny<OPECipher>(), It.IsAny<OPECipher>()))
				.Returns<OPECipher, OPECipher>((c, d) => c >= d);

			scheme
				.Setup(s => s.IsLessOrEqual(It.IsAny<OPECipher>(), It.IsAny<OPECipher>()))
				.Returns<OPECipher, OPECipher>((c, d) => c <= d);

			return scheme;
		}
	}

	[Trait("Category", "Unit")]
	public class Client
	{
		private readonly Mock<Mediator> _mediator =
			new Mock<Mediator>(
				new Mock<AbsClient>().Object,
				new Mock<AbsParty>().Object
			);

		private readonly Mock<IOREScheme<OPECipher, BytesKey>> _scheme = ProtocolHelper.GetScheme();

		private readonly Client<IOREScheme<OPECipher, BytesKey>, OPECipher, BytesKey> _client;

		public Client()
		{
			_client = new Client<IOREScheme<OPECipher, BytesKey>, OPECipher, BytesKey>(_scheme.Object);
			_client.SetMediator(_mediator.Object);
		}

		[Fact]
		public void Handshake()
		{
			_mediator.CallBase = false;

			_client.RunHandshake();

			_mediator.Verify(
				m => m.SendToServer<Tuple<OPECipher, OPECipher>, object>(
					It.Is<MinMaxMessage<OPECipher>>(
						t =>
							t.Unpack().Item1.value == int.MinValue &&
							t.Unpack().Item2.value == int.MaxValue
					)
				)
			);
		}

		[Fact]
		public void Construction()
		{
			_mediator.CallBase = false;

			_client.RunConstruction(
				Enumerable.Repeat(1, 10).Select(i => new Simulation.Protocol.Record(i, "")).ToList()
			);

			_mediator.Verify(
				m => m.SendToServer<EncryptedRecord<OPECipher>, object>(
					It.Is<InsertMessage<OPECipher>>(
						t => t.Unpack().cipher.value == 1
					)
				),
				Times.Exactly(10)
			);
		}

		[Fact]
		public void Search()
		{
			_mediator.CallBase = false;

			_client.RunSearch(
				Enumerable.Repeat(1, 10).Select(i => new RangeQuery(i, i + 2)).ToList()
			);

			_mediator.Verify(
				m => m.SendToServer<Tuple<OPECipher, OPECipher>, List<string>>(
					It.Is<QueryMessage<OPECipher>>(
						t =>
							t.Unpack().Item1.value == 1 &&
							t.Unpack().Item2.value == 3
					)
				),
				Times.Exactly(10)
			);
		}

		[Fact]
		public void AcceptMessage()
		{
			long storage = 0;
			_client.ClientStorage += n => storage += n;

			var message = new Mock<IMessage<object>>();
			message.Setup(m => m.GetSize()).Returns(10);

			var response = _client.AcceptMessage<object, object>(message.Object);

			Assert.IsType<FinishMessage>(response);

			Assert.Equal(10, storage);
		}

		[Fact]
		public void PropagatesSchemeEvents()
		{
			bool operation = false;
			bool primitive = false;

			_client.OperationOcurred += n => operation = true;
			_client.PrimitiveUsed += (n, i) => primitive = true;

			_scheme.Raise(s => s.OperationOcurred += null, SchemeOperation.Comparison);
			_scheme.Raise(s => s.PrimitiveUsed += null, Primitive.Hash, false);

			Assert.True(operation);
			Assert.True(primitive);
		}
	}

	[Trait("Category", "Unit")]
	public class Server
	{
		private readonly Server<OPECipher> _server;

		private readonly Mock<IOREScheme<OPECipher, BytesKey>> _scheme = ProtocolHelper.GetScheme();

		private readonly Mock<Options<OPECipher>> _options;

		public Server()
		{
			_options = new Mock<Options<OPECipher>>(_scheme.Object, 60, null);
			_server = new Server<OPECipher>(_options.Object);
		}

		[Fact]
		public void Dispatcher()
		{
			bool insert = false;
			bool query = false;
			bool minmax = false;

			var mockServer = new Mock<Server<OPECipher>>(_options.Object);
			mockServer.CallBase = true;

			mockServer
				.Protected()
				.As<IServerProtected<OPECipher>>()
				.Setup(s => s.AcceptMessage(It.IsAny<MinMaxMessage<OPECipher>>()))
				.Callback<MinMaxMessage<OPECipher>>(m => minmax = true);

			mockServer
				.Protected()
				.As<IServerProtected<OPECipher>>()
				.Setup(s => s.AcceptMessage(It.IsAny<InsertMessage<OPECipher>>()))
				.Callback<InsertMessage<OPECipher>>(m => insert = true);

			mockServer
				.Protected()
				.As<IServerProtected<OPECipher>>()
				.Setup(s => s.AcceptMessage(It.IsAny<QueryMessage<OPECipher>>()))
				.Callback<QueryMessage<OPECipher>>(m => query = true);

			mockServer.Object.AcceptMessage<Tuple<OPECipher, OPECipher>, object>(
				new Mock<MinMaxMessage<OPECipher>>(
					new Tuple<OPECipher, OPECipher>(
						new OPECipher(0),
						new OPECipher(0)
					)
				).Object
			);

			mockServer.Object.AcceptMessage<EncryptedRecord<OPECipher>, object>(
				new Mock<InsertMessage<OPECipher>>(
					new EncryptedRecord<OPECipher>
					{
						cipher = new OPECipher(0),
						value = "0"
					}
				).Object
			);

			mockServer.Object.AcceptMessage<Tuple<OPECipher, OPECipher>, List<string>>(
				new Mock<QueryMessage<OPECipher>>(
					new Tuple<OPECipher, OPECipher>(
						new OPECipher(0),
						new OPECipher(0)
					)
				).Object
			);

			Assert.True(minmax);
			Assert.True(insert);
			Assert.True(query);
		}

		[Fact]
		public void PropagateEvent()
		{
			bool nodeVisited = false;

			_server.NodeVisited += n => nodeVisited = true;

			_options.Raise(o => o.NodeVisited += null, 0);

			Assert.True(nodeVisited);
		}

		private interface IServerProtected<C> where C : IGetSize
		{
			FinishMessage AcceptMessage(MinMaxMessage<C> message);
			FinishMessage AcceptMessage(InsertMessage<C> message);
			QueryResponseMessage AcceptMessage(QueryMessage<C> message);
		}
	}
}
