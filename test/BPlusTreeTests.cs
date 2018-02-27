using System;
using Xunit;
using DataStructures;

namespace test
{
    public class BPlusTreeTests
    {
        [Fact]
        public void BPlusTreeTest()
        {
			Assert.True(new BPlusTree().DummyMethod());
        }
    }
}
