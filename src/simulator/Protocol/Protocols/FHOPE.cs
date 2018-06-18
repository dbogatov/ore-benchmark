using System;
using System.Collections.Generic;
using DataStructures.BPlusTree;
using ORESchemes.FHOPE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace Simulation.Protocol.FHOPE
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

			cipher.max = null;
			cipher.min = null;

			return new FinishMessage();
		}
	}

	public class Client : SimpleORE.Client<FHOPEScheme, Ciphertext, State>
	{
		public Client(FHOPEScheme scheme) : base(scheme) { }

		public override void RunHandshake()
		{
			Func<int, Ciphertext> encryptFull = plaintext =>
			{
				var cipher = _scheme.Encrypt(plaintext, _key);
				cipher.min = _scheme.MinCiphertext(plaintext, _key);
				cipher.max = _scheme.MaxCiphertext(plaintext, _key);

				return cipher;
			};

			_mediator.SendToServer<
				SimpleORE.MinMaxMessage<Ciphertext>, Tuple<Ciphertext, Ciphertext>,
				FinishMessage, object>(
				new SimpleORE.MinMaxMessage<Ciphertext>(
					new Tuple<Ciphertext, Ciphertext>(
						encryptFull(Int32.MinValue),
						encryptFull(Int32.MaxValue)
					)
				)
			);
		}

		protected override Ciphertext EncryptForSearch(int plaintext)
		{
			var cipher = _scheme.Encrypt(plaintext, _key);
			cipher.min = _scheme.MinCiphertext(plaintext, _key);
			cipher.max = _scheme.MaxCiphertext(plaintext, _key);

			cipher.value = 0;

			return cipher;
		}

		protected override Ciphertext EncryptForConstruction(int plaintext)
		{
			var cipher = _scheme.Encrypt(plaintext, _key);
			cipher.min = _scheme.MinCiphertext(plaintext, _key);
			cipher.max = _scheme.MaxCiphertext(plaintext, _key);

			return cipher;
		}
	}

	public class Protocol : SimpleORE.Protocol<FHOPEScheme, Ciphertext, State>
	{
		public Protocol(
			Options<Ciphertext> options,
			FHOPEScheme scheme
		) : base(options, scheme)
		{
			_client = new Client(scheme);
			_server = new Server(options);

			SetupProtocol();
		}
	}
}
