using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.SSE.CJJKRS
{
	public class Protocol : AbsProtocol, ISSEProtocol
	{
		public Protocol(byte[] entropy, int elementsPerPage)
		{
			_client = new Client(entropy);
			_server = new Server(elementsPerPage);

			SetupProtocol();
		}

		ISSEClient ISSEProtocol.ExposeClient() => (Client)_client;
	}
}
