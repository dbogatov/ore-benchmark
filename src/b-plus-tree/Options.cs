using System;
using OPESchemes;

namespace DataStructures.BPlusTree
{
	public class Options
	{
		public int Branching { get; private set; }
		public double Occupancy { get; private set; }
		public OPESchemes.OPESchemes Scheme { get; private set; }

		public Options(
			OPESchemes.OPESchemes scheme = OPESchemes.OPESchemes.NoEncryption,
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
