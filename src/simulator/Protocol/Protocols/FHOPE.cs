using System;
using BPlusTree;
using Crypto.FHOPE;

namespace Simulation.Protocol.FHOPE
{
	internal class Server : SimpleORE.Server<Ciphertext>
	{
		public Server(Options<Ciphertext> options) : base(options) { }

		protected override FinishMessage AcceptMessage(InsertMessage<Ciphertext> message)
		{
			var content = message.Unpack();

			_tree.Insert(
				content.cipher,
				content.value
			);

			content.cipher.min = null;
			content.cipher.max = null;

			return new FinishMessage();
		}
	}

	public class Client : SimpleORE.Client<Crypto.FHOPE.Scheme, Ciphertext, State>
	{
		public Client(Crypto.FHOPE.Scheme scheme) : base(scheme) { }

		public override void RunHandshake()
		{
			Func<int, Ciphertext> encryptFull = plaintext =>
			{
				var cipher = _scheme.Encrypt(plaintext, _key);
				cipher.min = _scheme.MinCiphertext(plaintext, _key);
				cipher.max = _scheme.MaxCiphertext(plaintext, _key);

				return cipher;
			};

			_mediator.SendToServer<Tuple<Ciphertext, Ciphertext>, object>(
				new SimpleORE.MinMaxMessage<Ciphertext>(
					new Tuple<Ciphertext, Ciphertext>(
						encryptFull(Int32.MinValue),
						encryptFull(Int32.MaxValue)
					)
				)
			);

			OnQueryCompleted();
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

	public class Protocol : SimpleORE.Protocol<Crypto.FHOPE.Scheme, Ciphertext, State>
	{
		public Protocol(
			Options<Ciphertext> options,
			Crypto.FHOPE.Scheme scheme
		) : base(options, scheme)
		{
			_client = new Client(scheme);
			_server = new Server(options);

			SetupProtocol();
		}
	}
}
