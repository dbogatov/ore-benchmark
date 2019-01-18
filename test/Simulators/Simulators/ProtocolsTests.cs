using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BPlusTree;
using Moq;
using Crypto.Shared;
using Crypto.Shared.Primitives;
using Simulation;
using Simulation.Protocol;
using Simulation.Protocol.SimpleORE;
using Xunit;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class ProtocolsSimulation
	{
		[Theory]
		[InlineData(Stages.Handshake)]
		[InlineData(Stages.Construction)]
		[InlineData(Stages.Queries)]
		public void Simulator(Stages stage)
		{
			Expression<Action<IProtocol>> setup = null;

			switch (stage)
			{
				case Stages.Handshake:
					setup = p => p.RunHandshake();
					break;
				case Stages.Construction:
					setup = p => p.RunConstructionProtocol(It.IsAny<List<Simulation.Protocol.Record>>());
					break;
				case Stages.Queries:
					setup = p => p.RunQueryProtocol(It.IsAny<List<RangeQuery>>());
					break;
			}

			Mock<IProtocol> protocol = new Mock<IProtocol>();
			protocol
				.Setup(setup)
				.Callback(
					() =>
					{
						for (int i = 0; i < 3; i++)
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

							protocol.Raise(p => p.QueryCompleted += null);
						}
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

				MessagesSent = 2 * 3,
				CommunicationVolume = 3 * 3,

				MaxClientStorage = 20,

				SchemeOperations = 3 * 3,

				TotalPrimitiveOperations =
					new Dictionary<Primitive, long>()
					{
						{ Primitive.Hash, 2 * 3 }
					},
				PurePrimitiveOperations =
					new Dictionary<Primitive, long>()
					{
						{ Primitive.Hash, 1 * 3 }
					},

				PerQuerySubreports = Enumerable.Repeat(
					(AbsSubReport)new Report.SubReport
					{
						CacheSize = 10,

						IOs = 2,

						MessagesSent = 2,
						CommunicationVolume = 3,

						MaxClientStorage = 20,

						SchemeOperations = 3,

						TotalPrimitiveOperations =
							new Dictionary<Primitive, long>()
							{
								{ Primitive.Hash, 2 }
							},
						PurePrimitiveOperations =
							new Dictionary<Primitive, long>()
							{
								{ Primitive.Hash, 1 }
							},
					},
					3
				).ToList()
			};

			Report.SubReport actual = (Report.SubReport)report.Stages[stage];

			CompareReports(expected, actual);

			void CompareReports(Report.SubReport expectedReport, Report.SubReport actualReport, bool goDeeper = true)
			{
				Assert.Equal(expected.CacheSize, actual.CacheSize);

				Assert.Equal(expected.IOs, actual.IOs);

				Assert.Equal(expected.MessagesSent, actual.MessagesSent);
				Assert.Equal(expected.CommunicationVolume, actual.CommunicationVolume);

				Assert.Equal(expected.MaxClientStorage, actual.MaxClientStorage);

				Assert.Equal(expected.SchemeOperations, actual.SchemeOperations);

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

				if (goDeeper)
				{
					for (int i = 0; i < expectedReport.PerQuerySubreports.Count; i++)
					{
						CompareReports(
							(Report.SubReport)expectedReport.PerQuerySubreports[i], 
							(Report.SubReport)actualReport.PerQuerySubreports[i],
							goDeeper: false
						);
					}
				}
				else
				{
					Assert.Empty(actualReport.PerQuerySubreports);
				}
			}
		}

		[Fact]
		public void Integration()
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

			var description = report.ToString();

			foreach (var subreport in report.Stages.Values.Cast<Report.SubReport>())
			{
				Assert.Contains(subreport.SchemeOperations.ToString(), description);
			}
		}
	}
}
