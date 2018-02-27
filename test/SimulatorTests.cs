using System;
using Xunit;
using Simulation;

namespace test
{
    public class SimulatorTests
    {
        [Fact]
        public void SimulatorTest()
        {
			Assert.True(new Simulator().DummyMethod());
        }
    }
}
