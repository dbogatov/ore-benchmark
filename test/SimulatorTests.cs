using System;
using Xunit;
using Simulation;

namespace Test
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
