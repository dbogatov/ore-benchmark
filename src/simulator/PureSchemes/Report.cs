namespace Simulation.PureSchemes
{
	public enum Stages
	{
		Encrypt, Decrypt, Compare
	}

	public class Report : AbsReport<Stages>
	{
		public class SubReport : AbsSubReport
		{
			public long MaxCipherSize { get; set; } = 0;
			public long MaxStateSize { get; set; } = 0;

			public override string ToString() =>
				$@"
		Maximal cipher size recorded {MaxCipherSize} bits
		Maximal state size recorded {MaxStateSize} bits

		Primitive usage for input of size {SchemeOperations}:

{PrintPrimitiveUsage()}
		Observable time elapsed: {ObservedTime}
	";
		}
	}
}
