namespace Simulation.Protocol
{
	public enum Stages
	{
		Handshake, Construction, Queries
	}

	public class Report : AbsReport<Stages>
	{
		public class SubReport : AbsSubReport
		{
			public int CacheSize { get; set; } = 0;

			public long IOs { get; set; } = 0;
			public long AvgIOs { get; set; } = 0;

			public long MessagesSent { get; set; } = 0;
			public long CommunicationVolume { get; set; } = 0;
			public long AvgMessageSize => CommunicationVolume == 0 ? 0 : CommunicationVolume / MessagesSent;

			public long MaxClientStorage { get; set; } = 0;

			public override string ToString()
			{
				return $@"
		Number of I/O operations (for cache size {CacheSize}): {IOs} | {AvgIOs} per query
		Number of ORE scheme operations performed: {SchemeOperations} | {AvgSchemeOperations} per query
		Sent {MessagesSent} messages, {CommunicationVolume / 8} byte(s) communication ({AvgMessageSize / 8} byte(s) per message)
		Client storage went to max of {MaxClientStorage / 8} byte(s)
		
{PrintPrimitiveUsage()}
		Observable time elapsed: {ObservedTime}
";
			}
		}
	}
}
