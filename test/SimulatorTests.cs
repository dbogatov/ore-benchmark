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
			Simulator<int, string>.Simulate(new Inputs<int, string>());
        }
    }
}
