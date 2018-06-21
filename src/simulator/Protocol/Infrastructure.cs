using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol
{
	/// <summary>
	/// Encapsulates a party of a protocol capable of exchanging messages.
	/// Eq. client and server
	/// </summary>
	public abstract class AbsParty : AbsEventHandler
	{
		protected Mediator _mediator;

		/// <summary>
		/// Sets a link to mediator through which messages should be exchanged
		/// </summary>
		public void SetMediator(Mediator mediator) => _mediator = mediator;

		/// <summary>
		/// Generic routine responsible for reacting to messages
		/// </summary>
		/// <param name="message">Message to react to</param>
		/// <typeparam name="Q">Type of request message's content</typeparam>
		/// <typeparam name="R">Type of response message's content</typeparam>
		/// <returns>A response message</returns>
		public abstract IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message);
	}

	public abstract class AbsClient : AbsParty
	{
		/// <summary>
		/// Initiates handshake protocol stage
		/// </summary>
		public abstract void RunHandshake();

		/// <summary>
		/// Initiates construction protocol stage
		/// </summary>
		public abstract void RunConstruction(List<Record> input);

		/// <summary>
		/// Initiates search protocol stage
		/// </summary>
		public abstract void RunSearch(List<RangeQuery> input);

		/// <summary>
		/// Trigger to make a recording of self storage plus optional value
		/// </summary>
		/// <param name="extra">Optional number of bits to add to current storage for report</param>
		public virtual void RecordStorage(long extra = 0) => OnClientStorage(extra);
	}

	public interface IMessage<out T>
	{
		/// <summary>
		/// Extract message's content
		/// </summary>
		T Unpack();

		/// <summary>
		/// Returns the size of the message
		/// </summary>
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

	/// <summary>
	/// Special kind of message signalizing OK response of 0 bits
	/// </summary>
	/// <typeparam name="object"></typeparam>
	public class FinishMessage : AbsMessage<object>
	{
		public FinishMessage() : base(null) { }
		public FinishMessage(object content) : base(content) { }

		public override int GetSize() => 0;
	}

	/// <summary>
	/// Entity responsible for passing messages between protocol parties
	/// and keeping track of messages' count and size
	/// </summary>
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

		/// <summary>
		/// Passes message to server.
		/// Parameters correspond to party's AcceptMessage routine.
		/// </summary>
		public virtual IMessage<R> SendToServer<Q, R>(IMessage<Q> message)
		{
			OnMessageSent(message.GetSize());

			var response = _server.AcceptMessage<Q, R>(message);
			_client.RecordStorage(response.GetSize());

			OnMessageSent(response.GetSize());

			return response;
		}

		/// <summary>
		/// Passes message to client.
		/// Parameters correspond to party's AcceptMessage routine.
		/// </summary>
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

		/// <summary>
		/// Initiates construction protocol stage
		/// </summary>
		void RunConstructionProtocol(List<Record> input);

		/// <summary>
		/// Initiates search protocol stage
		/// </summary>
		void RunQueryProtocol(List<RangeQuery> input);

		/// <summary>
		/// Initiates handshake protocol stage
		/// </summary>
		void RunHandshake();
	}

	public abstract class AbsProtocol : AbsEventHandler, IProtocol
	{
		protected AbsClient _client;
		protected AbsParty _server;

		protected Mediator _mediator;

		/// <summary>
		/// Sets up the protocol object.
		/// In particular, hooks up events.
		/// </summary>
		/// <param name="mediator">If provided, new mediator will not be created</param>
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

	/// <summary>
	/// Abstraction that contains events of intrest and routines to trigger those events
	/// </summary>
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
