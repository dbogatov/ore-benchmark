using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.BPlusTree;
using ORESchemes.CryptDBOPE;
using ORESchemes.FHOPE;
using ORESchemes.LewiORE;
using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using Simulation.Protocol;
using Simulation.Protocol.SimpleORE;
using Xunit;

namespace Test.Simulators.Protocols.Integration
{
	[Trait("Category", "Unit")]
	public class NoEncryptionProtocolTests : AbsProtocolTests
	{
		public NoEncryptionProtocolTests()
		{
			var scheme = new NoEncryptionScheme();
			_protocol = new Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
				new Options<OPECipher>(scheme), scheme
			);
		}
	}

	[Trait("Category", "Unit")]
	public class CryptDBProtocolTests : AbsProtocolTests
	{
		public CryptDBProtocolTests()
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
		}
	}

	[Trait("Category", "Unit")]
	public class PracticalOREProtocolTests : AbsProtocolTests
	{
		public PracticalOREProtocolTests()
		{
			var scheme = new PracticalOREScheme();

			_protocol = new Protocol<PracticalOREScheme, global::ORESchemes.PracticalORE.Ciphertext, BytesKey>(
				new Options<global::ORESchemes.PracticalORE.Ciphertext>(scheme), scheme
			);
		}
	}

	[Trait("Category", "Unit")]
	public class LewiOREProtocolTests : AbsProtocolTests
	{
		public LewiOREProtocolTests()
		{
			var scheme = new LewiOREScheme();

			_protocol = new Simulation.Protocol.LewiORE.Protocol(
				new Options<global::ORESchemes.LewiORE.Ciphertext>(scheme), scheme
			);
		}
	}

	[Trait("Category", "Unit")]
	public class FHOPEProtocolTests : AbsProtocolTests
	{
		public FHOPEProtocolTests()
		{
			var scheme = new FHOPEScheme(long.MinValue, long.MaxValue);

			_protocol = new Simulation.Protocol.FHOPE.Protocol(
				new Options<global::ORESchemes.FHOPE.Ciphertext>(scheme), scheme
			);
		}
	}
}
