using System;

namespace Simulation.Protocol.ORAM
{
	internal abstract class BucketMessage : AbsMessage<ValueTuple<byte[], int>>
	{
		public BucketMessage(ValueTuple<byte[], int> content) : base(content) { }

		public override int GetSize() => _content.Item1.Length * sizeof(byte);
	}

	internal class WriteBucketMessage : BucketMessage
	{
		public WriteBucketMessage(ValueTuple<byte[], int> content) : base(content) { }
	}

	internal class ReadBucketMessage : BucketMessage
	{
		public ReadBucketMessage(ValueTuple<byte[], int> content) : base(content) { }
	}
}
