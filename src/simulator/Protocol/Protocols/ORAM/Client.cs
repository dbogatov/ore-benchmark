using System;
using System.Collections.Generic;
using System.Text;
using BPlusTree;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;

namespace Simulation.Protocol.ORAM
{
	public class Client : AbsClient
	{
		/// <summary>
		/// A helper class that defines trivial comparison rules for B+ tree
		/// </summary>
		private class Comparator : IOREComparator<int>
		{
			public event SchemeOperationEventHandler OperationOcurred;
			public event PrimitiveUsageEventHandler PrimitiveUsed;

			public bool IsEqual(int ciphertextOne, int ciphertextTwo) => ciphertextOne == ciphertextTwo;

			public bool IsGreater(int ciphertextOne, int ciphertextTwo) => ciphertextOne > ciphertextTwo;

			public bool IsGreaterOrEqual(int ciphertextOne, int ciphertextTwo) => ciphertextOne >= ciphertextTwo;

			public bool IsLess(int ciphertextOne, int ciphertextTwo) => ciphertextOne < ciphertextTwo;

			public bool IsLessOrEqual(int ciphertextOne, int ciphertextTwo) => ciphertextOne <= ciphertextTwo;
		}

		private readonly ISymmetric E;
		private readonly IPRG G;
		private readonly byte[] _key;
		private readonly Tree<string, int> _tree;
		private readonly int _branches;
		private readonly int _z;
		private readonly byte[] _gibberish;

		public Client(byte[] entropy, int branches = 1024, int z = 4)
		{
			E = new SymmetricFactory().GetPrimitive();
			G = new PRGFactory().GetPrimitive();

			_key = G.GetBytes(128 / 8);
			_gibberish = E.Encrypt(_key, Encoding.Default.GetBytes("Gibberish"));

			G.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			E.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);

			var options =
				new Options<int>(
					new Comparator(),
					branches
				);

			_tree = new Tree<string, int>(options);
			_branches = branches;
			_z = z;
			
			options.NodeAccessHandler = AccessORAM;

			OnClientStorage(_key.Length * 8);
		}

		public override void RunConstruction(List<Record> input)
		{
			foreach (var record in input)
			{
				_tree.Insert(record.index, record.value);

				SampleTreeSize();

				OnQueryCompleted();
			}
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				List<string> result = new List<string>();
				_tree.TryRange(
					query.from,
					query.to,
					result,
					checkRanges: true
				);
				
				SampleTreeSize();
				
				OnQueryCompleted();
			}
		}

		/// <summary>
		/// Records the current size of the B+ tree
		/// </summary>
		private void SampleTreeSize() =>
			OnClientStorage(
				_tree.Nodes(includeDataNodes: false) * _branches * sizeof(int) * 8 + _key.Length * 8
			);

		/// <summary>
		/// Handler to use when a B+ tree nodes gets accessed.
		/// In particular, it initiates a communication with the ORAM server.
		/// </summary>
		/// <param name="hash"></param>
		private void AccessORAM(int hash)
		{
			var size = _tree.Size();
			var heightORAM = (int)Math.Ceiling(Math.Log(size, 2));

			foreach (var readOrWrite in new bool[] { true, false })
			{
				for (int i = 0; i < heightORAM; i++)
				{
					if (readOrWrite)
					{
						E.Decrypt(_key, _gibberish);
						_mediator.SendToServer<ValueTuple<byte[], int>, object>(
							new ReadBucketMessage((new byte[_z * _branches * sizeof(int)], size))
						);
					}
					else
					{
						E.Encrypt(_key, Encoding.Default.GetBytes("Re-encrypt data"));
						_mediator.SendToServer<ValueTuple<byte[], int>, object>(
							new WriteBucketMessage((new byte[_z * _branches * sizeof(int)], size))
						);
					}
				}
			}
		}
	}
}