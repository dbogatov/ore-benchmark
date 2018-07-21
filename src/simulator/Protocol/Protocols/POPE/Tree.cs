using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Protocol.POPE
{
	internal class Tree
	{
		private readonly Node _root = new LeafNode();

		public void Insert(EncryptedRecord<Cipher> block) => _root.Insert(block);

		private abstract class Node
		{
			protected readonly HashSet<EncryptedRecord<Cipher>> _buffer = new HashSet<EncryptedRecord<Cipher>>();

			public abstract void Insert(EncryptedRecord<Cipher> block);

			internal abstract List<Cipher> GetAllCiphers();
		}

		private class InternalNode : Node
		{
			private readonly List<Cipher> _list = new List<Cipher>();

			private readonly List<Node> _children = new List<Node>();

			public override void Insert(EncryptedRecord<Cipher> block)
			{
				throw new NotImplementedException();
			}

			internal override List<Cipher> GetAllCiphers() =>
				_list.Concat(_buffer.Select(b => b.cipher)).Concat(_children.Select(c => c.GetAllCiphers()).SelectMany(c => c)).ToList();
		}

		private class LeafNode : Node
		{
			public override void Insert(EncryptedRecord<Cipher> block) => _buffer.Add(block);

			internal override List<Cipher> GetAllCiphers() => _buffer.Select(b => b.cipher).ToList();
		}

		internal bool ValidateElementsInserted(List<int> expected, Func<Cipher, int> decode)
		{
			expected = expected.OrderBy(c => c).ToList();
			var actual = _root.GetAllCiphers().Select(c => decode(c)).OrderBy(c => c).ToList();

			return expected.Zip(actual, (a, b) => a == b).All(c => c);
		}
	}
}
