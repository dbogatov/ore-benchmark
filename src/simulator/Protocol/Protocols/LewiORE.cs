using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.LewiORE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol.LewiORE
{
	public class Server : SimpleORE.Server<Ciphertext>
	{
		public Server(Options<Ciphertext> options) : base(options) { }

		protected override FinishMessage AcceptMessage(SimpleORE.InsertMessage<Ciphertext> message)
		{
			var cipher = message.Unpack();

			_tree.Insert(
				cipher,
				""
			);

			cipher.left = null;

			return new FinishMessage();
		}

		protected override SimpleORE.QueryResultMessage AcceptMessage(SimpleORE.QueryMessage<Ciphertext> message)
		{
			List<string> result = new List<string>();
			_tree.TryRange(
				message.Unpack().Item1,
				message.Unpack().Item2,
				out result
			);

			return new SimpleORE.QueryResultMessage(result);
		}
	}

	public class Client : SimpleORE.Client<LewiOREScheme, Ciphertext, Key>
	{
		public Client(LewiOREScheme scheme) : base(scheme) { }

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				_mediator.SendToServer<
					SimpleORE.QueryMessage<Ciphertext>, Tuple<Ciphertext, Ciphertext>,
					SimpleORE.QueryResultMessage, List<string>>(
					new SimpleORE.QueryMessage<Ciphertext>(
						new Tuple<Ciphertext, Ciphertext>(
							new Ciphertext { left = _scheme.EncryptLeft(_key.left, _key.right, query.from.ToUInt()) },
							new Ciphertext { left = _scheme.EncryptLeft(_key.left, _key.right, query.to.ToUInt()) }
						)
					)
				);
			}
		}
	}

	public class Protocol : SimpleORE.Protocol<LewiOREScheme, Ciphertext, Key>
	{
		public Protocol(
			Options<Ciphertext> options,
			LewiOREScheme scheme
		) : base(options, scheme)
		{
			_client = new Client(scheme);
			_server = new Server(options);

			SetupProtocol();
		}
	}
}
