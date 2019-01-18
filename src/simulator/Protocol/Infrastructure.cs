using System;
using System.Collections.Generic;
using System.Diagnostics;
using Crypto.Shared;
using Crypto.Shared.Primitives;

namespace Simulation.Protocol
{
	/// <summary>
	/// Encapsulates a party of a protocol capable of exchanging messages.
	/// e.g. client and server
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
		public virtual void RunHandshake() => OnQueryCompleted();

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

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			OnClientStorage(message.GetSize());

			return (IMessage<R>)new FinishMessage();
		}
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

		public override int GetSize() => 0;
	}

	public class RequestMessage : AbsMessage<object>
	{
		public RequestMessage() : base(null) { }

		public override int GetSize() => 0;
	}

	public class QueryResponseMessage : AbsMessage<List<string>>
	{
		public QueryResponseMessage(List<string> content) : base(content) { }

		public override int GetSize() => 0; // do not include response size
	}

	public class InsertMessage<C> : SizeableMessage<EncryptedRecord<C>> where C : IGetSize
	{
		public InsertMessage(EncryptedRecord<C> content) : base(content) { }
	}

	public class QueryMessage<C> : AbsMessage<Tuple<C, C>> where C : IGetSize
	{
		public QueryMessage(Tuple<C, C> content) : base(content) { }

		public override int GetSize() => _content.Item1.GetSize() + _content.Item2.GetSize();
	}

	public class SizeableMessage<T> : AbsMessage<T> where T : IGetSize
	{
		public SizeableMessage(T content) : base(content) { }

		public override int GetSize() => _content.GetSize();
	}

	public class EncryptedRecord<C> : IGetSize where C : IGetSize
	{
		public C cipher;
		public string value;

		/// <summary>
		/// Size of encrypted value
		/// 0 if we do not count values in message size
		/// </summary>
		private readonly int VALUESIZE = 0;

		public int GetSize() => cipher.GetSize() + VALUESIZE;
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
				party.MessageSent += OnMessageSent;
				party.NodeVisited += OnNodeVisited;
				party.OperationOcurred += OnOperationOccurred;
				party.PrimitiveUsed += OnPrimitiveUsed;
				party.ClientStorage += OnClientStorage;
				party.Timer += OnTimer;
				party.QueryCompleted += OnQueryCompleted;
			}
		}

		/// <summary>
		/// Passes message to server.
		/// Parameters correspond to party's AcceptMessage routine.
		/// </summary>
		public virtual IMessage<R> SendToServer<Q, R>(IMessage<Q> message)
		{
			StopTimer(() =>
				OnMessageSent(message.GetSize())
			);

			var response = _server.AcceptMessage<Q, R>(message);

			StopTimer(() =>
			{
				_client.RecordStorage(response.GetSize());
				OnMessageSent(response.GetSize());
			});

			return response;
		}

		/// <summary>
		/// Passes message to client.
		/// Parameters correspond to party's AcceptMessage routine.
		/// </summary>
		public virtual IMessage<R> SendToClient<Q, R>(IMessage<Q> message)
		{
			StopTimer(() =>
			{
				OnMessageSent(message.GetSize());
				_client.RecordStorage(message.GetSize());
			});

			var response = _client.AcceptMessage<Q, R>(message);

			StopTimer(() =>
				OnMessageSent(response.GetSize())
			);

			return response;
		}

		/// <summary>
		/// Stops timer, executes routine and resumes timer
		/// </summary>
		private void StopTimer(Action routine)
		{
			OnTimer(stop: true);

			routine();

			OnTimer(stop: false);
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
		/// Event signalizing whether to stop or resume simulation timer
		/// </summary>
		event TimerEventHandler Timer;

		/// <summary>
		/// Event signalizing that a single query has been completed
		/// </summary>
		event QueryCompletedHandler QueryCompleted;

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
			Debug.Assert(_client != null);
			Debug.Assert(_server != null);
			
			_mediator = mediator ?? new Mediator(_client, _server);

			_client.SetMediator(_mediator);
			_server.SetMediator(_mediator);

			_mediator.MessageSent += OnMessageSent;
			_mediator.NodeVisited += OnNodeVisited;
			_mediator.OperationOcurred += OnOperationOccurred;
			_mediator.PrimitiveUsed += OnPrimitiveUsed;
			_mediator.ClientStorage += OnClientStorage;
			_mediator.Timer += OnTimer;
			_mediator.QueryCompleted += OnQueryCompleted;
		}

		public virtual void RunConstructionProtocol(List<Record> input)
		{
			ResumeTimer(() =>
				_client.RunConstruction(input)
			);
		}

		public virtual void RunHandshake()
		{
			ResumeTimer(() =>
				_client.RunHandshake()
			);
		}

		public virtual void RunQueryProtocol(List<RangeQuery> input)
		{
			ResumeTimer(() =>
				_client.RunSearch(input)
			);
		}

		/// <summary>
		/// Starts timer, executes routine and stop timer
		/// </summary>
		private void ResumeTimer(Action routine)
		{
			OnTimer(stop: false);

			routine();

			OnTimer(stop: true);
		}
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
		public virtual event TimerEventHandler Timer;
		public virtual event QueryCompletedHandler QueryCompleted;

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

		protected void OnTimer(bool stop)
		{
			var handler = Timer;
			if (handler != null)
			{
				handler(stop);
			}
		}

		protected void OnQueryCompleted()
		{
			var handler = QueryCompleted;
			if (handler != null)
			{
				handler();
			}
		}
	}

	public delegate void MessageSentEventHandler(long size);
	public delegate void ClientStorageEventHandler(long size);
	public delegate void TimerEventHandler(bool stop);
	public delegate void QueryCompletedHandler();
}
