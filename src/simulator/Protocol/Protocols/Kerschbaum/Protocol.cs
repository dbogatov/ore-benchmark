using System.Runtime.CompilerServices;
using Crypto.Shared;
using Crypto.Shared.Primitives.PRG;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.Kerschbaum
{
	public class Protocol : AbsProtocol
	{
		public Protocol(byte[] entropy, int blockSize = 60)
		{
			IPRG G = new PRGFactory(entropy).GetPrimitive();

			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8), blockSize);

			SetupProtocol();
		}
	}

	internal class Cipher : IGetSize
	{
		public byte[] encrypted;

		public int GetSize() => encrypted.Length * 8;

		public override int GetHashCode() => encrypted.GetHashCode();
	}

	internal class InsertContent : IGetSize
	{
		public Cipher index;
		public string value;
		public int location;

		public int GetSize() => sizeof(byte) + index.GetSize() + sizeof(int) * 8;
	}
}
