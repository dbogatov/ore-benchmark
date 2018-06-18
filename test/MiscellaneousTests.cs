using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using Simulation.Protocol;

namespace Test
{
	[Trait("Category", "Unit")]
	public class MiscellaneousTests
	{
		[Fact]
		public void PrintByteExtensionTest()
		{
			var bytes = new byte[] { 0x00, 0x13, 0x05, 0x19, 0x96, 0xAA };

			var description = bytes.Print();

			foreach (var b in bytes)
			{
				Assert.Contains(b.ToString(), description);
			}
		}

		[Fact]
		public void RecordPrintMethods()
		{
			Random generator = new Random(123456);

			for (int i = 0; i < 10; i++)
			{
				var index = generator.Next();
				var value = generator.Next().ToString();
				Simulation.Protocol.Record record = new Simulation.Protocol.Record(index, value);
				Assert.Contains(index.ToString(), record.ToString());
				Assert.Contains(value.ToString(), record.ToString());
			}
		}
	}
}
