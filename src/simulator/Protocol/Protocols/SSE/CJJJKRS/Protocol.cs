using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Protocol : AbsProtocol, ISSEProtocol
	{
		public Protocol(byte[] entropy, int elementsPerPage)
		{
			_client = new Client(entropy, elementsPerPage);
			_server = new Server(elementsPerPage);

			SetupProtocol();
		}

		ISSEClient ISSEProtocol.ExposeClient() => (Client)_client;
	}
}
