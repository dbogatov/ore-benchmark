using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol.SimpleORE
{
	public class InsertMessage<C> : AbsMessage<C> where C : IGetSize
	{
		public InsertMessage(C content) : base(content) { }

		public override int GetSize() => _content.GetSize();
	}

	public class QueryMessage<C> : AbsMessage<Tuple<C, C>> where C : IGetSize
	{
		public QueryMessage(Tuple<C, C> content) : base(content) { }

		public override int GetSize() => _content.Item1.GetSize() + _content.Item2.GetSize();
	}

	public class QueryResultMessage : AbsMessage<List<string>>
	{
		public QueryResultMessage(List<string> content) : base(content) { }

		public override int GetSize() => _content.Count * sizeof(byte);
	}

	public class MinMaxMessage<C> : AbsMessage<Tuple<C, C>> where C : IGetSize
	{
		public MinMaxMessage(Tuple<C, C> content) : base(content) { }

		public override int GetSize() => _content.Item1.GetSize() + _content.Item2.GetSize();
	}

	public class Server<C> : AbsParty where C : IGetSize
	{
		protected readonly Options<C> _options;
		protected readonly Tree<string, C> _tree;


		public Server(Options<C> options)
		{
			_options = options;
			_tree = new Tree<string, C>(_options);

			_options.NodeVisited += new NodeVisitedEventHandler(OnNodeVisited);
		}

		protected virtual FinishMessage AcceptMessage(InsertMessage<C> message)
		{
			_tree.Insert(
				message.Unpack(),
				""
			);

			return new FinishMessage();
		}

		protected virtual QueryResultMessage AcceptMessage(QueryMessage<C> message)
		{
			List<string> result = new List<string>();
			_tree.TryRange(
				message.Unpack().Item1,
				message.Unpack().Item2,
				out result
			);

			return new QueryResultMessage(result);
		}

		protected virtual FinishMessage AcceptMessage(MinMaxMessage<C> message)
		{
			_options.MinCipher = message.Unpack().Item1;
			_options.MaxCipher = message.Unpack().Item2;

			return new FinishMessage();
		}

		public override MR AcceptMessage<MQ, TQ, MR, TR>(MQ message)
		{
			var msgType = message.GetType();

			if (msgType == typeof(InsertMessage<C>))
			{
				return (MR)(object)AcceptMessage((InsertMessage<C>)(object)message);
			}

			if (msgType == typeof(QueryMessage<C>))
			{
				return (MR)(object)AcceptMessage((QueryMessage<C>)(object)message);
			}

			if (msgType == typeof(MinMaxMessage<C>))
			{
				return (MR)(object)AcceptMessage((MinMaxMessage<C>)(object)message);
			}

			return (MR)(object)new FinishMessage();
		}
	}

	public class Client<S, C, K> : AbsClient
		where S : IOREScheme<C, K>
		where C : IGetSize
		where K : IGetSize
	{
		protected S _scheme;
		protected K _key;

		public Client(S scheme)
		{
			_scheme = scheme;

			_scheme.Init();
			_key = _scheme.KeyGen();

			_scheme.OperationOcurred += new SchemeOperationEventHandler(OnOperationOccurred);
			_scheme.PrimitiveUsed += new PrimitiveUsageEventHandler(OnPrimitiveUsed);

			OnClientStorage(_key.GetSize());
		}

		public override MR AcceptMessage<MQ, TQ, MR, TR>(MQ message)
		{
			OnClientStorage(message.GetSize());

			return (MR)(object)new FinishMessage();
		}

		public override void RunConstruction(List<Record> input)
		{
			foreach (var record in input)
			{
				_mediator.SendToServer<
					InsertMessage<C>, C,
					FinishMessage, object>(
					new InsertMessage<C>(
						EncryptForConstruction(record.index)
					)
				);
			}
		}

		public override void RunHandshake()
		{
			_mediator.SendToServer<
				MinMaxMessage<C>, Tuple<C, C>,
				FinishMessage, object>(
				new MinMaxMessage<C>(
					new Tuple<C, C>(
						_scheme.MinCiphertextValue(_key),
						_scheme.MaxCiphertextValue(_key)
					)
				)
			);
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				_mediator.SendToServer<
					QueryMessage<C>, Tuple<C, C>,
					QueryResultMessage, List<string>>(
					new QueryMessage<C>(
						new Tuple<C, C>(
							EncryptForSearch(query.from),
							EncryptForSearch(query.to)
						)
					)
				);
			}
		}

		protected virtual C EncryptForConstruction(int plaintext) => _scheme.Encrypt(plaintext, _key);
		protected virtual C EncryptForSearch(int plaintext) => _scheme.Encrypt(plaintext, _key);
	}

	public class Protocol<S, C, K> : AbsProtocol
		where S : IOREScheme<C, K>
		where C : IGetSize
		where K : IGetSize
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
