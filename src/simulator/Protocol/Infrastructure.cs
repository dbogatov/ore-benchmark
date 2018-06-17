using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol
{
	public abstract class AbsParty<E> : AbsEventHandler where E : Enum // where E : System.Enum
	{
		protected Mediator<E> _mediator;

		public void SetMediator(Mediator<E> mediator) => _mediator = mediator;

		public abstract AbsMessage<E> AcceptMessage(AbsMessage<E> message);
	}

	public abstract class AbsClient<E> : AbsParty<E> where E : Enum
	{
		public abstract void RunHandshake();
		public abstract void RunConstruction(List<Record> input);
		public abstract void RunSearch(List<RangeQuery> input);
	}

	public abstract class AbsMessage<E> where E : Enum
	{
		protected object _content;
		public readonly E Type;

		public AbsMessage(E type)
		{
			Type = type;
		}

		public AbsMessage(object content, E type)
		{
			_content = content;
			Type = type;
		}

		public object Unpack() => _content;

		public abstract int GetSize();
	}

	public class Mediator<E> : AbsEventHandler where E : Enum
	{
		private AbsParty<E> _client;
		private AbsParty<E> _server;

		public Mediator(AbsParty<E> client, AbsParty<E> server)
		{
			_client = client;
			_server = server;

			foreach (var party in new AbsParty<E>[] { _client, _server })
			{
				party.MessageSent += new MessageSentEventHandler(OnMessageSent);
				party.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
				party.OperationOcurred += new SchemeOperationEventHandler(OnOperationOccurred);
				party.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitiveUsed);
				party.ClientStorage += new ClientStorageEventHandler(OnClientStorage);
			}
		}

		public AbsMessage<E> SendToServer(AbsMessage<E> message)
		{
			OnMessageSent(message.GetSize());

			return _server.AcceptMessage(message);
		}

		public AbsMessage<E> SendToClient(AbsMessage<E> message)
		{
			OnMessageSent(message.GetSize());

			return _client.AcceptMessage(message);
		}
	}

	public interface IProtocol
	{
		event NodeVisitedEventHandler NodeVisited;
		event SchemeOperationEventHandler OperationOcurred;
		event PrimitiveUsageEventHandler PrimitiveUsed;
		event MessageSentEventHandler MessageSent;
		event ClientStorageEventHandler ClientStorage;

		void RunConstructionProtocol(List<Record> input);
		void RunQueryProtocol(List<RangeQuery> input);
		void RunHandshake();
	}

	public abstract class AbsProtocol<E> : AbsEventHandler, IProtocol where E : Enum
	{
		protected AbsClient<E> _client;
		protected AbsParty<E> _server;

		protected Mediator<E> _mediator;

		protected void SetupProtocol()
		{
			if (_client == null || _server == null)
			{
				throw new InvalidOperationException();
			}

			_mediator = new Mediator<E>(_client, _server);

			_client.SetMediator(_mediator);
			_client.SetMediator(_mediator);

			_mediator.MessageSent += new MessageSentEventHandler(OnMessageSent);
			_mediator.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
			_mediator.OperationOcurred += new SchemeOperationEventHandler(OnOperationOccurred);
			_mediator.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitiveUsed);
			_mediator.ClientStorage += new ClientStorageEventHandler(OnClientStorage);
		}

		public virtual void RunConstructionProtocol(List<Record> input) => _client.RunConstruction(input);

		public virtual void RunHandshake() => _client.RunHandshake();

		public virtual void RunQueryProtocol(List<RangeQuery> input) => _client.RunSearch(input);


	}

	public abstract class AbsEventHandler
	{
		public event NodeVisitedEventHandler NodeVisited;
		public event SchemeOperationEventHandler OperationOcurred;
		public event PrimitiveUsageEventHandler PrimitiveUsed;
		public event MessageSentEventHandler MessageSent;
		public event ClientStorageEventHandler ClientStorage;

		protected void OnMessageSent(long size)
		{
			var handler = MessageSent;
			if (handler != null)
			{
				handler(size);
			}
		}

		protected void OnNodeVisited(int hash)
		{
			var handler = NodeVisited;
			if (handler != null)
			{
				handler(hash);
			}
		}

		protected void OnOperationOccurred(SchemeOperation operation)
		{
			var handler = OperationOcurred;
			if (handler != null)
			{
				handler(operation);
			}
		}

		protected void OnPrimitiveUsed(Primitive primitive, bool impure)
		{
			var handler = PrimitiveUsed;
			if (handler != null)
			{
				handler(primitive, impure);
			}
		}

		protected void OnClientStorage(long size)
		{
			var handler = ClientStorage;
			if (handler != null)
			{
				handler(size);
			}
		}
	}

	public delegate void MessageSentEventHandler(long size);

	public delegate void ClientStorageEventHandler(long size);
}
