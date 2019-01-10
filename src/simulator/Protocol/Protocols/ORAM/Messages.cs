namespace Simulation.Protocol.ORAM
{
	internal abstract class BucketMessage : AbsMessage<(byte[] buckets, int level, int total)>
	{
		public BucketMessage((byte[] buckets, int level, int total) content) : base(content) { }

		public override int GetSize() => _content.Item1.Length * sizeof(byte);
	}

	internal class WriteBucketMessage : BucketMessage
	{
		public WriteBucketMessage((byte[] buckets, int level, int total) content) : base(content) { }
	}

	internal class ReadBucketMessage : BucketMessage
	{
		public ReadBucketMessage((byte[] buckets, int level, int total) content) : base(content) { }
	}
}
