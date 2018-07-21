using System.Runtime.CompilerServices;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.POPE
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
}
