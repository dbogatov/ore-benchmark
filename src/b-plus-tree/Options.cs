using System;
using ORESchemes.Shared;

namespace DataStructures.BPlusTree
{
	public delegate void NodeVisitedEventHandler(int nodeHash);

	/// <typeparam name="C">Ciphertext type</typeparam>
	public class Options<C>
	{
		public event NodeVisitedEventHandler NodeVisited;

		public int Branching { get; private set; }
		public IOREComparator<C> Comparator { get; private set; }

		public C MaxCipher;
		public C MinCipher;

		private int _generator = 0;

		public Options(
			IOREComparator<C> comparator,
			int branching = 60
		)
		{
			if (branching < 2 || branching > 65536)
			{
				throw new ArgumentException("Bad B+ tree options");
			}

			Branching = branching;

			Comparator = comparator;
		}

		/// <summary>
		/// Emits event when node has been visited
		/// </summary>
		/// <param name="hash">Unique hash of the node</param>
		public void OnVisit(int hash)
		{
			var handler = NodeVisited;
			if (handler != null)
			{
				handler(hash);
			}
		}

		/// <summary>
		/// Returns the next unique available id for a node
		/// </summary>
		public int GetNextId() => _generator++;
	}
}
