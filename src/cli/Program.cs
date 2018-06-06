using McMaster.Extensions.CommandLineUtils;

namespace CLI
{
	public class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<SimulatorCommand>(args);
	}
}
