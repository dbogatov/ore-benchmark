using DataStructures.BPlusTree;
using ORESchemes.LewiORE;
using ORESchemes.Shared;

namespace Simulation.Protocol.LewiORE
{
	public class Server : SimpleORE.Server<Ciphertext>
	{
		public Server(Options<Ciphertext> options) : base(options) { }

		protected override FinishMessage AcceptMessage(InsertMessage<Ciphertext> message)
		{
			var content = message.Unpack();

			_tree.Insert(
				content.cipher,
				content.value
			);

			content.cipher.left = null;

			return new FinishMessage();
		}
	}

	public class Client : SimpleORE.Client<LewiOREScheme, Ciphertext, Key>
	{
		public Client(LewiOREScheme scheme) : base(scheme) { }

		protected override Ciphertext EncryptForSearch(int plaintext) =>
			new Ciphertext { left = _scheme.EncryptLeft(_key.left, _key.right, plaintext.ToUInt()) };
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
