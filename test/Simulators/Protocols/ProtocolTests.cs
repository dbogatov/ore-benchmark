using System;
using System.Collections.Generic;
using System.Linq;
using BPlusTree;
using Crypto.BCLO;
using Crypto.FHOPE;
using Crypto.LewiWu;
using Crypto.CLWW;
using Crypto.Shared;
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
				new global::Crypto.BCLO.Scheme(
					Int32.MinValue,
					Int32.MaxValue,
					Convert.ToInt64(-Math.Pow(2, 48)),
					Convert.ToInt64(Math.Pow(2, 48))
				);

			_protocol = new Protocol<global::Crypto.BCLO.Scheme, OPECipher, BytesKey>(
				new Options<OPECipher>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class CLWWProtocol : AbsProtocol
	{
		public CLWWProtocol()
		{
			var scheme = new global::Crypto.CLWW.Scheme();

			_protocol = new Protocol<global::Crypto.CLWW.Scheme, global::Crypto.CLWW.Ciphertext, BytesKey>(
				new Options<global::Crypto.CLWW.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class LewiWuProtocol : AbsProtocol
	{
		public LewiWuProtocol()
		{
			var scheme = new global::Crypto.LewiWu.Scheme();

			_protocol = new Simulation.Protocol.LewiWu.Protocol(
				new Options<global::Crypto.LewiWu.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class FHOPEProtocol : AbsProtocol
	{
		public FHOPEProtocol()
		{
			var scheme = new global::Crypto.FHOPE.Scheme(long.MinValue, long.MaxValue);

			_protocol = new Simulation.Protocol.FHOPE.Protocol(
				new Options<global::Crypto.FHOPE.Ciphertext>(scheme), scheme
			);

			SetupHandlers();
		}
	}

	[Trait("Category", "Unit")]
	public class KerschbaumProtocol : AbsProtocol
	{
		public KerschbaumProtocol()
		{
			_protocol = new global::Simulation.Protocol.Kerschbaum.Protocol(new Random(123456).GetBytes(128 / 8));

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

	[Trait("Category", "Unit")]
	public class ORAMProtocol : AbsProtocol
	{
		public ORAMProtocol()
		{
			_protocol = new global::Simulation.Protocol.ORAM.Protocol(new Random(123456).GetBytes(128 / 8), 64);

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
	public class CJJKRSProtocol : AbsProtocol
	{
		public CJJKRSProtocol()
		{
			_protocol = new global::Simulation.Protocol.SSE.CJJKRS.Protocol(new Random(123456).GetBytes(128 / 8), 64);

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
	public class CJJJKRSProtocol : AbsProtocol
	{
		public CJJJKRSProtocol()
		{
			_protocol = new global::Simulation.Protocol.SSE.CJJJKRS.Protocol(new Random(123456).GetBytes(128 / 8), 64);

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
