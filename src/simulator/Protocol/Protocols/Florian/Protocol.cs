using System.Runtime.CompilerServices;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.Florian
{
	public class Protocol : AbsProtocol
	{
		public Protocol(byte[] entropy)
		{
			IPRG G = new PRGFactory(entropy).GetPrimitive();

			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8));

			SetupProtocol();
		}
	}

	internal class Cipher : IGetSize
	{
		public byte[] encrypted;

		public int GetSize() => encrypted.Length * 8;
	}

	internal class InsertContent : IGetSize
	{
		public Cipher index;
		public string value;
		public int location;

		public int GetSize() => sizeof(byte) + index.GetSize() + sizeof(int) * 8;
	}
}
