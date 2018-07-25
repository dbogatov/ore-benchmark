using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.BPlusTree;
using ORESchemes.CryptDBOPE;
using ORESchemes.FHOPE;
using ORESchemes.LewiORE;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using Simulation.Protocol.SimpleORE;
using Xunit;

namespace Test.Simulators.Protocols.Integration
{
	[Trait("Category", "Unit")]
	public class NoEncryptionProtocol : AbsProtocol
	{
		public NoEncryptionProtocol()
		{
			var scheme = new NoEncryptionScheme();
			_protocol = new Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
				new Options<OPECipher>(scheme), scheme
			);

			SetupHandlers();
		}

		protected override HashSet<Events> ExpectedTriggers
		{
			get
			{
				var set = Enum.GetValues(typeof(Events)).Cast<Events>().ToHashSet();
				set.Remove(Events.PrimitiveUsage);
				return set;
			}
		}
	}

	[Trait("Category", "Unit")]
	public class CryptDBProtocol : AbsProtocol
	{
		public CryptDBProtocol()
		{
			var scheme =
				new CryptDBScheme(
					Int32.MinValue,
					Int32.MaxValue,
					Convert.ToInt64(-Math.Pow(2, 48)),
					Convert.ToInt64(Math.Pow(2, 48))
				);

			_protocol = new Protocol<CryptDBScheme, OPECipher, BytesKey>(
				new Options<OPECipher>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class PracticalOREProtocol : AbsProtocol
	{
		public PracticalOREProtocol()
		{
			var scheme = new PracticalOREScheme();

			_protocol = new Protocol<PracticalOREScheme, global::ORESchemes.PracticalORE.Ciphertext, BytesKey>(
				new Options<global::ORESchemes.PracticalORE.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class LewiOREProtocol : AbsProtocol
	{
		public LewiOREProtocol()
		{
			var scheme = new LewiOREScheme();

			_protocol = new Simulation.Protocol.LewiORE.Protocol(
				new Options<global::ORESchemes.LewiORE.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class FHOPEProtocol : AbsProtocol
	{
		public FHOPEProtocol()
		{
			var scheme = new FHOPEScheme(long.MinValue, long.MaxValue);

			_protocol = new Simulation.Protocol.FHOPE.Protocol(
				new Options<global::ORESchemes.FHOPE.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class FlorianProtocol : AbsProtocol
	{
		public FlorianProtocol()
		{
			_protocol = new global::Simulation.Protocol.Florian.Protocol(new Random(123456).GetBytes(128 / 8));

			SetupHandlers();
		}

		protected override HashSet<Events> ExpectedTriggers
		{
			get
			{
				var set = Enum.GetValues(typeof(Events)).Cast<Events>().ToHashSet();
				set.Remove(Events.SchemeOperation);
				return set;
			}
		}
	}

	[Trait("Category", "Unit")]
	public class POPEProtocol : AbsProtocol
	{
		public POPEProtocol()
		{
			_protocol = new global::Simulation.Protocol.POPE.Protocol(new Random(123456).GetBytes(128 / 8));

			SetupHandlers();
		}

		protected override HashSet<Events> ExpectedTriggers
		{
			get
			{
				var set = Enum.GetValues(typeof(Events)).Cast<Events>().ToHashSet();
				set.Remove(Events.SchemeOperation);
				return set;
			}
		}
	}
}
