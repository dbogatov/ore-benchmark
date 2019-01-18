using System;
using Crypto.Shared.Primitives;
using Crypto.Shared.Primitives.PRG;

namespace Simulation.Protocol.ORAM
{
	public class Server : AbsParty
	{
		private readonly IPRG G;
		private readonly int _z;
		private readonly int _elementsPerPage;

		public Server(byte[] entropy, int z, int elementsPerPage)
		{
			G = new PRGFactory(entropy).GetPrimitive();
			_z = z;
			_elementsPerPage = elementsPerPage;
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
			=> (IMessage<R>)AcceptMessage((BucketMessage)message);

		/// <summary>
		/// React to a message from ORAM client.
		/// In this fake version of ORAM, just report I/Os that would have
		/// been made for a real PathORAM.
		/// </summary>
		/// <param name="message">A write or read buck message</param>
		/// <returns>A stub finish message</returns>
		private FinishMessage AcceptMessage(BucketMessage message)
		{
			OnPrimitiveUsed(Primitive.ORAMLevel, false);

			// total number of buckets assuming balanced binary tree of given height
			var totalBuckets = Math.Pow(2, message.Unpack().total + 1) - 1;
			
			// total number of I/O pages occupied by all buckets given the number of buckets per page
			var totalPages = totalBuckets / _elementsPerPage;
			
			// if pages are a contiguous array, compute the range endpoints given tree level
			var pageRangeMin = totalPages * ((Math.Pow(2, message.Unpack().level) - 1) / totalBuckets);
			var pageRangeMax = totalPages * ((Math.Pow(2, message.Unpack().level + 1) - 1) / totalBuckets);

			// if Z bucket occupy more than one page, then how many
			var pagesAccessed = (int)Math.Ceiling(1.0 * _z / _elementsPerPage);

			// use PRG to drive a location of the page within the range
			var hash = G.Next((int)Math.Ceiling(pageRangeMin), (int)Math.Ceiling(pageRangeMax));

			// is more than one page is accessed (due to Z), simulate it
			for (int i = 0; i < pagesAccessed; i++)
			{
				OnNodeVisited((hash + i) % (int)Math.Ceiling(pageRangeMax));
			}

			return new FinishMessage();
		}
	}
}
