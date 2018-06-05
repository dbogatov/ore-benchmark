using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using Simulation.BPlusTree;

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

		[Theory]
		[InlineData(QueriesType.Exact)]
		[InlineData(QueriesType.Delete)]
		[InlineData(QueriesType.Range)]
		[InlineData(QueriesType.Update)]
		public void InputsPrintMethods(QueriesType type)
		{
			Random generator = new Random(123456);

			for (int i = 0; i < 10; i++)
			{
				switch (type)
				{
					case QueriesType.Exact:
						var exactIndex = generator.Next();
						ExactQuery exact = new ExactQuery(exactIndex);
						Assert.Contains(exactIndex.ToString(), exact.ToString());
						break;
					case QueriesType.Delete:
						var deleteIndex = generator.Next();
						DeleteQuery delete = new DeleteQuery(deleteIndex);
						Assert.Contains(deleteIndex.ToString(), delete.ToString());
						break;
					case QueriesType.Range:
						var rangeFrom = generator.Next();
						var rangeTo = generator.Next();
						RangeQuery range = new RangeQuery(rangeFrom, rangeTo);
						Assert.Contains(rangeFrom.ToString(), range.ToString());
						Assert.Contains(rangeTo.ToString(), range.ToString());
						break;
					case QueriesType.Update:
						var updateIndex = generator.Next();
						var newValue = generator.Next().ToString();
						UpdateQuery<string> update = new UpdateQuery<string>(updateIndex, newValue);
						Assert.Contains(updateIndex.ToString(), update.ToString());
						Assert.Contains(newValue.ToString(), update.ToString());
						break;
				}
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
				Record<string> record = new Record<string>(index, value);
				Assert.Contains(index.ToString(), record.ToString());
				Assert.Contains(value.ToString(), record.ToString());
			}
		}
	}
}
