using System.Threading.Tasks;
using System.Threading;
using Xunit;
using System;
using System.IO;

namespace Test.Web.UnitTests
{
	[Trait("Category", "Unit")]
	public class EntryPoint
	{
		[Fact]
		public void NoExceptions()
		{
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				var task = global::Web.Program.Entrypoint(new string[] { 52525.ToString() }, cts.Token);
				Thread.Sleep(3 * 1000);
				cts.Cancel();
			}
		}

		[Fact]
		public async Task IncorrectPort()
		{
			Console.SetOut(TextWriter.Null);
			Assert.NotEqual(0, await global::Web.Program.Main(new string[] { 1000.ToString() }));
			Assert.NotEqual(0, await global::Web.Program.Main(new string[] { 70000.ToString() }));
		}
	}
}
