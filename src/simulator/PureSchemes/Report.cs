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

			public override string ToString() =>
				$@"
		Primitive usage for input of size {SchemeOperations}:

{PrintPrimitiveUsage()}
		Observable time elapsed: {ObservedTime}
	";
		}
	}
}
