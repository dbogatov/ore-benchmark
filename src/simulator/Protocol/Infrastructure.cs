using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol
{
	public abstract class AbsParty : AbsEventHandler
	{
		protected Mediator _mediator;

		public void SetMediator(Mediator mediator) => _mediator = mediator;

		public abstract IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message);
	}

	public abstract class AbsClient : AbsParty
	{
		public abstract void RunHandshake();
		public abstract void RunConstruction(List<Record> input);
		public abstract void RunSearch(List<RangeQuery> input);

		public virtual void RecordStorage(long extra = 0) => OnClientStorage(extra);
	}

	public interface IMessage<out T>
	{
		T Unpack();
		int GetSize();
	}

	public abstract class AbsMessage<T> : IMessage<T>
	{
		protected T _content;

		public AbsMessage() { }

		public AbsMessage(T content)
		{
			_content = content;
		}

		public T Unpack() => _content;

		public abstract int GetSize();
	}

	public class FinishMessage : AbsMessage<object>
	{
		public FinishMessage() : base(null) { }
		public FinishMessage(object content) : base(content) { }

		public override int GetSize() => 0;
	}

	public class Mediator : AbsEventHandler
	{
		private AbsClient _client;
		private AbsParty _server;

		public Mediator(AbsClient client, AbsParty server)
		{
			_client = client;
			_server = server;

			foreach (var party in new AbsParty[] { _client, _server })
			{
				party.MessageSent += new MessageSentEventHandler(OnMessageSent);
				party.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
				party.OperationOcurred += new SchemeOperationEventHandler(OnOperationOccurred);
				party.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitiveUsed);
				party.ClientStorage += new ClientStorageEventHandler(OnClientStorage);
			}
		}

		public virtual IMessage<R> SendToServer<Q, R>(IMessage<Q> message)
		{
			OnMessageSent(message.GetSize());

			var response = _server.AcceptMessage<Q, R>(message);
			_client.RecordStorage(response.GetSize());

			OnMessageSent(response.GetSize());

			return response;
		}

		public virtual IMessage<R> SendToClient<Q, R>(IMessage<Q> message)
		{
			OnMessageSent(message.GetSize());
			_client.RecordStorage(message.GetSize());

			var response = _client.AcceptMessage<Q, R>(message);

			OnMessageSent(response.GetSize());

			return response;
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

	public abstract class AbsProtocol : AbsEventHandler, IProtocol
	{
		protected AbsClient _client;
		protected AbsParty _server;

		protected Mediator _mediator;

		protected void SetupProtocol(Mediator mediator = null)
		{
			if (_client == null || _server == null)
			{
				throw new InvalidOperationException();
			}

			_mediator = mediator ?? new Mediator(_client, _server);

			_client.SetMediator(_mediator);
			_server.SetMediator(_mediator);

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
		public virtual event NodeVisitedEventHandler NodeVisited;
		public virtual event SchemeOperationEventHandler OperationOcurred;
		public virtual event PrimitiveUsageEventHandler PrimitiveUsed;
		public virtual event MessageSentEventHandler MessageSent;
		public virtual event ClientStorageEventHandler ClientStorage;

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
