using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol.PracticalORE
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

	public class Server : AbsParty<Messages>
	{
		private readonly Options<Ciphertext> _options;
		private readonly Tree<string, Ciphertext> _tree;


		public Server(Options<Ciphertext> options)
		{
			_options = options;
			_tree = new Tree<string, Ciphertext>(_options);

			_options.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
		}

		public override AbsMessage<Messages> AcceptMessage(AbsMessage<Messages> message)
		{
			switch (message.Type)
			{
				case Messages.Insert:
					_tree.Insert(
						(Ciphertext)(message).Unpack(),
						""
					);
					break;
				case Messages.Query:
					List<string> result = new List<string>();
					_tree.TryRange(
						((Tuple<Ciphertext, Ciphertext>)message.Unpack()).Item1,
						((Tuple<Ciphertext, Ciphertext>)message.Unpack()).Item2,
						out result
					);
					return new Message(result, Messages.QueryResult);
				case Messages.MinMax:
					_options.MinCipher = ((Tuple<Ciphertext, Ciphertext>)message.Unpack()).Item1;
					_options.MaxCipher = ((Tuple<Ciphertext, Ciphertext>)message.Unpack()).Item2;
					break;
			}

			return new Message();
		}
	}

	public class Client : AbsClient<Messages>
	{
		private PracticalOREScheme _scheme;
		private byte[] _key;

		public Client(PracticalOREScheme scheme)
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
					new Tuple<Ciphertext, Ciphertext>(
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
						new Tuple<Ciphertext, Ciphertext>(
							_scheme.Encrypt(query.from, _key),
							_scheme.Encrypt(query.to, _key)
						),
						Messages.Query
					)
				);
			}
		}
	}

	public class Protocol : AbsProtocol<Messages>
	{
		public Protocol(
			Options<Ciphertext> options,
			PracticalOREScheme scheme
		)
		{
			_client = new Client(scheme);
			_server = new Server(options);

			SetupProtocol();
		}
	}
}
