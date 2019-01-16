using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.SSE.CJJJKRS
{
	public class Protocol : AbsProtocol, ISSEProtocol
	{
		public Protocol(byte[] entropy, int elementsPerPage, int b = int.MaxValue, int B = 1)
		{
			_client = new Client(entropy, b, B);
			_server = new Server(elementsPerPage, b, B);

			SetupProtocol();
		}

		ISSEClient ISSEProtocol.ExposeClient() => (Client)_client;
	}
}
