using System;
using OPESchemes;

namespace DataStructures.BPlusTree
{
	public class Options<P, C>
	{
		public int Branching { get; private set; }
		public double Occupancy { get; private set; }
		public IOPEScheme<P, C> Scheme { get; private set; }

		public Options(
			IOPEScheme<P, C> scheme,
			int branching = 60,
			double occupancy = 0.7
		)
		{
			if (
				branching < 3 || branching > 65536 ||
				occupancy < 0.5 || occupancy > 0.9
			)
			{
				throw new ArgumentException("Bad B+ tree options");
			}

			Branching = branching;
			Occupancy = occupancy;

			Scheme = scheme;
		}
	}
}
