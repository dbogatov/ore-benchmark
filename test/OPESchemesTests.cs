using System;
using Xunit;
using OPESchemes;

namespace test
{
    public class OPESchemesTests
    {
        [Fact]
        public void CryptDBTest()
        {
			Assert.True(new CryptDBScheme().DummyMethod());
        }
    }
}
