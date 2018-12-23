using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Test.Web
{
	[Trait("Category", "Unit")]

	public partial class Integration
	{
		public Integration()
		{
			// Ensure no exceptions
			new TestServer(
				new WebHostBuilder()
					.UseStartup<global::Web.Startup>()
			);
		}

		[Fact]
		public void IntegrationUnit() { }
	}
}
