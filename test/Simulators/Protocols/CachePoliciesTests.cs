using Simulation;
using Simulation.Protocol;
using Xunit;

namespace Test.Simulators.Protocols
{
	public class CachePolicies
	{
		private readonly int RUNS = 100;

		[Fact]
		public void LRU()
		{
			var tracker = new Tracker(RUNS, CachePolicy.LRU);

			// fill up the cache
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Null(tracker.RecordNodeVisit(i));
			}

			// reverse direction to differ from FIFO
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Null(tracker.RecordNodeVisit(RUNS - i - 1));
			}

			// check eviction
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(RUNS - i - 1, tracker.RecordNodeVisit(RUNS + i));
			}
		}

		[Fact]
		public void LFU()
		{
			var tracker = new Tracker(RUNS, CachePolicy.LFU);

			// fill up the cache
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Null(tracker.RecordNodeVisit(i));
			}

			// add frequencies
			for (int i = 0; i < RUNS; i++)
			{
				for (int j = 0; j < RUNS - i - 1; j++)
				{
					Assert.Null(tracker.RecordNodeVisit(i));
				}
			}

			// check eviction
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(RUNS - i - 1, tracker.RecordNodeVisit(RUNS + i));
				for (int j = 0; j < RUNS + 5; j++)
				{
					Assert.Null(tracker.RecordNodeVisit(RUNS + i));
				}
			}
		}

		[Fact]
		public void FIFO()
		{
			var tracker = new Tracker(RUNS, CachePolicy.FIFO);

			// fill up the cache
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Null(tracker.RecordNodeVisit(i));
			}

			// add weight to old ones (differ from LFU)
			for (int i = 0; i < RUNS; i++)
			{
				for (int j = 0; j < RUNS - i + 5; j++)
				{
					Assert.Null(tracker.RecordNodeVisit(i));
				}
			}

			// reverse direction to differ from LRU
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Null(tracker.RecordNodeVisit(RUNS - i - 1));
			}

			// check eviction
			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(i, tracker.RecordNodeVisit(RUNS + i));
			}
		}
	}
}
