using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataStructures.BPlusTree;
using Moq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using Simulation.Protocol;
using Simulation.Protocol.SimpleORE;
using Xunit;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class ProtocolsTests
	{
		[Theory]
		[InlineData(Stages.Handshake)]
		[InlineData(Stages.Construction)]
		[InlineData(Stages.Queries)]
		public void SimulatorTest(Stages stage)
		{
			Expression<Action<IProtocol>> setup = null;
			int actionsNumber = 0;

			switch (stage)
			{
				case Stages.Handshake:
					setup = p => p.RunHandshake();
					actionsNumber = 1;
					break;
				case Stages.Construction:
					setup = p => p.RunConstructionProtocol(It.IsAny<List<Simulation.Protocol.Record>>());
					actionsNumber = 10;
					break;
				case Stages.Queries:
					setup = p => p.RunQueryProtocol(It.IsAny<List<RangeQuery>>());
					actionsNumber = 10;
					break;
			}

			Mock<IProtocol> protocol = new Mock<IProtocol>();
			protocol
				.Setup(setup)
				.Callback(
					() =>
					{
						protocol.Raise(p => p.ClientStorage += null, 20);
						protocol.Raise(p => p.ClientStorage += null, 10);

						protocol.Raise(p => p.MessageSent += null, 1);
						protocol.Raise(p => p.MessageSent += null, 2);

						protocol.Raise(p => p.NodeVisited += null, 5);
						protocol.Raise(p => p.NodeVisited += null, 10);
						protocol.Raise(p => p.NodeVisited += null, 5);

						protocol.Raise(p => p.OperationOcurred += null, SchemeOperation.Comparison);
						protocol.Raise(p => p.OperationOcurred += null, SchemeOperation.Encrypt);
						protocol.Raise(p => p.OperationOcurred += null, SchemeOperation.Encrypt);

						protocol.Raise(p => p.PrimitiveUsed += null, Primitive.Hash, true);
						protocol.Raise(p => p.PrimitiveUsed += null, Primitive.Hash, false);
					}
				);

			Inputs inputs = new Inputs
			{
				Dataset = Enumerable.Range(1, 10).Select(i => new Simulation.Protocol.Record(i, "")).ToList(),
				Queries = Enumerable.Range(1, 10).Select(i => new RangeQuery(i, i + 1)).ToList(),
				CacheSize = 10
			};

			Simulator simulator = new Simulator(inputs, protocol.Object);

			Report report = (Report)simulator.Simulate();

			Report.SubReport expected = new Report.SubReport
			{
				CacheSize = 10,

				IOs = 2,
				AvgIOs = 2 / actionsNumber,

				MessagesSent = 2,
				CommunicationVolume = 3,

				MaxClientStorage = 20,

				SchemeOperations = 3,
				AvgSchemeOperations = 3 / actionsNumber,

				TotalPrimitiveOperations =
					new Dictionary<Primitive, long>()
					{
						{ Primitive.Hash, 2 }
					},
				PurePrimitiveOperations =
					new Dictionary<Primitive, long>()
					{
						{ Primitive.Hash, 1 }
					}
			};

			Report.SubReport actual = (Report.SubReport)report.Stages[stage];

			CompareReports(expected, actual);

			void CompareReports(Report.SubReport expectedReport, Report.SubReport actualReport)
			{
				Assert.Equal(expected.CacheSize, actual.CacheSize);

				Assert.Equal(expected.IOs, actual.IOs);
				Assert.Equal(expected.AvgIOs, actual.AvgIOs);

				Assert.Equal(expected.MessagesSent, actual.MessagesSent);
				Assert.Equal(expected.CommunicationVolume, actual.CommunicationVolume);

				Assert.Equal(expected.MaxClientStorage, actual.MaxClientStorage);

				Assert.Equal(expected.SchemeOperations, actual.SchemeOperations);
				Assert.Equal(expected.AvgSchemeOperations, actual.AvgSchemeOperations);

				ComparePrimitiveUsage(
					expectedReport.TotalPrimitiveOperations,
					actualReport.TotalPrimitiveOperations
				);

				ComparePrimitiveUsage(
					expectedReport.PurePrimitiveOperations,
					actualReport.PurePrimitiveOperations
				);

				void ComparePrimitiveUsage(
					Dictionary<Primitive, long> expectedPrimitives,
					Dictionary<Primitive, long> actualPrimitives
				)
				{
					foreach (var key in Enum.GetValues(typeof(Primitive)).Cast<Primitive>())
					{
						if (expectedPrimitives.ContainsKey(key))
						{
							Assert.Equal(expectedPrimitives[key], actualPrimitives[key]);
						}
						else
						{
							Assert.Equal(0, actualPrimitives[key]);
						}
					}
				}
			}
		}

		[Fact]
		public void IntegrationTest()
		{
			Random random = new Random(123456);

			Inputs inputs = new Inputs
			{
				Dataset = Enumerable.Range(1, 20).Select(i => new Simulation.Protocol.Record(i, "")).ToList(),
				Queries = Enumerable.Range(1, 20).Select(i => new RangeQuery(random.Next(1, 10), random.Next(10, 20))).ToList(),
				CacheSize = 10
			};

			var scheme = new NoEncryptionScheme();
			var protocol = new Protocol<NoEncryptionScheme, OPECipher, BytesKey>(
				new Options<OPECipher>(scheme), scheme
			);

			var simulator = new Simulator(inputs, protocol);

			var report = simulator.Simulate();

			foreach (var subreport in report.Stages.Values.Cast<Report.SubReport>())
			{
				Assert.NotEqual(0, subreport.SchemeOperations);
				Assert.NotEqual(0, subreport.CommunicationVolume);
				Assert.NotEqual(0, subreport.MessagesSent);
			}

			var descriptions = new List<string> {
				report.ToString(),
				report.ToConciseString()
			};

			foreach (var description in descriptions)
			{
				foreach (var subreport in report.Stages.Values.Cast<Report.SubReport>())
				{
					Assert.Contains(subreport.SchemeOperations.ToString(), description);
				}
			}
		}
	}
}
