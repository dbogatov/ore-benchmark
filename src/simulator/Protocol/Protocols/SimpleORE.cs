using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol.SimpleORE
{
	public enum Messages
	{
		Finish, Insert, Query, QueryResult, MinMax
	}

	public class Message : AbsMessage<Messages>
	{
		public Message() : base(Messages.Finish) { }
		public Message(object content, Messages type) : base(content, type) { }

		// TODO
		public override int GetSize()
		{
			switch (Type)
			{
				case Messages.Insert:

					return 5;
				case Messages.Query:

					return 10;
				case Messages.QueryResult:

					return 20;
				default:
					return 0;
			}

		}
	}

	public class Server<C> : AbsParty<Messages>
	{
		private readonly Options<C> _options;
		private readonly Tree<string, C> _tree;


		public Server(Options<C> options)
		{
			_options = options;
			_tree = new Tree<string, C>(_options);

			_options.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
		}

		public override AbsMessage<Messages> AcceptMessage(AbsMessage<Messages> message)
		{
			switch (message.Type)
			{
				case Messages.Insert:
					_tree.Insert(
						(C)(message).Unpack(),
						""
					);
					break;
				case Messages.Query:
					List<string> result = new List<string>();
					_tree.TryRange(
						((Tuple<C, C>)message.Unpack()).Item1,
						((Tuple<C, C>)message.Unpack()).Item2,
						out result
					);
					return new Message(result, Messages.QueryResult);
				case Messages.MinMax:
					_options.MinCipher = ((Tuple<C, C>)message.Unpack()).Item1;
					_options.MaxCipher = ((Tuple<C, C>)message.Unpack()).Item2;
					break;
			}

			return new Message();
		}
	}

	public class Client<S, C, K> : AbsClient<Messages> where S : IOREScheme<C, K>
	{
		private S _scheme;
		private K _key;

		public Client(S scheme)
		{
			_scheme = scheme;

			_scheme.Init();
			_key = _scheme.KeyGen();

			_scheme.OperationOcurred += new SchemeOperationEventHandler(OnOperationOccurred);
			_scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitiveUsed);

			// TODO: make key report its size
			OnClientStorage(2 * 32 + 256);
		}

		public override AbsMessage<Messages> AcceptMessage(AbsMessage<Messages> message)
		{
			return new Message();
		}

		public override void RunConstruction(List<Record> input)
		{
			foreach (var record in input)
			{
				_mediator.SendToServer(
					new Message(
						_scheme.Encrypt(record.index, _key),
						Messages.Insert
					)
				);
			}
		}

		public override void RunHandshake()
		{
			_mediator.SendToServer(
				new Message(
					new Tuple<C, C>(
						_scheme.MinCiphertextValue(_key),
						_scheme.MaxCiphertextValue(_key)
					),
					Messages.MinMax
				)
			);
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				_mediator.SendToServer(
					new Message(
						new Tuple<C, C>(
							_scheme.Encrypt(query.from, _key),
							_scheme.Encrypt(query.to, _key)
						),
						Messages.Query
					)
				);
			}
		}
	}

	public class Protocol<S, C, K> : AbsProtocol<Messages> where S : IOREScheme<C, K>
	{
		public Protocol(
			Options<C> options,
			S scheme
		)
		{
			_client = new Client<S, C, K>(scheme);
			_server = new Server<C>(options);

			SetupProtocol();
		}
	}
}
