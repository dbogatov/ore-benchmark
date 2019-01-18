using System.Runtime.CompilerServices;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.ORAM
{
	public class Protocol : AbsProtocol
	{
		public Protocol(byte[] entropy, int elementsPerPage, int branches = 256, int z = 4)
		{
			IPRG G = new PRGFactory(entropy).GetPrimitive();

			_client = new Client(G.GetBytes(128 / 8), branches, z);
			_server = new Server(G.GetBytes(128 / 8), z, elementsPerPage);

			SetupProtocol();
		}
	}
}
