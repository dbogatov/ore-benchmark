using System;
using Xunit;

namespace Test.Web.UnitTests
{
	[Trait("Category", "Unit")]
	public class EnvironmentTest
	{
		[Fact]
		public void TestingEnvironmentSet()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			Assert.Equal("Testing", env);
		}
	}
}
