using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.POPE
{
	internal class Options
	{
		public int L;
		public Action<HashSet<Cipher>> SetList;
		public Func<List<Cipher>> GetSortedList;
		public Func<Cipher, int> IndexToInsert;
		public Func<Cipher, int> IndexOfResult;
		public IPRG G;
	}

	internal class Tree
	{
		private readonly Node _root;

		public Tree(Options options)
		{
			_root = new LeafNode(options);
		}

		public void Insert(EncryptedRecord<Cipher> block) => _root.Insert(block);

		public List<string> Search(Cipher left, Cipher right)
		{

			LeafNode leftLeaf = _root.Split(left);
			LeafNode rightLeaf = _root.Split(right);

			List<string> result = new List<string>();

			do
			{
				result.AddRange(leftLeaf._buffer.Select(b => b.value));
			} while (leftLeaf != rightLeaf);

			return result;
		}

		private abstract class Node
		{
			public Node parent = null;
			public Node right = null;
			public Node left = null;

			protected readonly Options _options;

			public Node(Options options)
			{
				_options = options;
			}

			public readonly HashSet<EncryptedRecord<Cipher>> _buffer = new HashSet<EncryptedRecord<Cipher>>();

			public void Insert(EncryptedRecord<Cipher> block) => _buffer.Add(block);

			public abstract LeafNode Split(Cipher label);

			public void AcceptChild(EncryptedRecord<Cipher> child) => _buffer.Add(child);

			internal abstract List<Cipher> GetAllCiphers();
		}

		private class InternalNode : Node
		{
			private readonly List<CipherChild> _children = new List<CipherChild>();

			public InternalNode(Options options) : base(options) { }

			public override LeafNode Split(Cipher label)
			{
				_options.SetList(new HashSet<Cipher>(_children.Select(c => c.cipher)));

				if (_buffer.Count != 0)
				{
					foreach (var block in _buffer)
					{
						var index = _options.IndexToInsert(block.cipher);
						_children[index].child.AcceptChild(block);
					}
					_buffer.Clear();
				}

				var resultIndex = _options.IndexOfResult(label);
				return _children[resultIndex].child.Split(label);
			}

			public LeafNode AcceptChildren(LeafNode child, HashSet<EncryptedRecord<Cipher>> buffer, List<Cipher> list, Cipher label)
			{
				list.Add(null);
				var toInsert = list.Select(c => new CipherChild { cipher = c, child = new LeafNode(_options) }).ToList();
				for (int i = 0; i < toInsert.Count; i++)
				{
					if (i != 0)
					{
						toInsert[i].child.left = toInsert[i - 1].child;
					}
					if (i != toInsert.Count)
					{
						toInsert[i].child.right = toInsert[i + 1].child;
					}

					toInsert[i].child.parent = this;
				}

				if (_children.Count == 0)
				{
					_children.AddRange(toInsert);
				}
				else
				{
					for (int i = 0; i < _children.Count; i++)
					{
						if (_children[i].child == child)
						{
							var thisChild = _children[i].child;

							if (thisChild.left != null)
							{
								thisChild.left.right = toInsert.First().child;
							}

							if (thisChild.right != null)
							{
								thisChild.right.left = toInsert.Last().child;
							}

							_children.RemoveAt(i);
							_children.InsertRange(i, toInsert);

							break;
						}
					}
				}

				foreach (var block in buffer)
				{
					var index = _options.IndexToInsert(block.cipher);
					_children[index].child.AcceptChild(block);
				}

				var resultIndex = _options.IndexOfResult(label);
				return (LeafNode)_children[resultIndex].child;
			}

			internal override List<Cipher> GetAllCiphers() =>
				_children.Select(c => c.cipher).Concat(_buffer.Select(b => b.cipher)).Concat(_children.Select(c => c.child.GetAllCiphers()).SelectMany(c => c)).ToList();
		}

		private class LeafNode : Node
		{
			public LeafNode(Options options) : base(options)
			{
			}

			public override LeafNode Split(Cipher label)
			{
				if (_buffer.Count <= _options.L)
				{
					return this;
				}

				HashSet<Cipher> labels = new HashSet<Cipher>();

				for (int i = 0; i < _options.L; i++)
				{
					var sampled = _buffer.ElementAt(_options.G.Next(0, _buffer.Count));
					_buffer.Remove(sampled);
					labels.Add(sampled.cipher);
				}

				_options.SetList(labels);

				InternalNode newRoot;

				if (parent == null)
				{
					newRoot = new InternalNode(_options);
				}
				else
				{
					newRoot = (InternalNode)parent;
				}

				var sorted = _options.GetSortedList();

				return ((InternalNode)parent).AcceptChildren(this, _buffer, sorted, label);
			}

			internal override List<Cipher> GetAllCiphers() => _buffer.Select(b => b.cipher).ToList();
		}

		private class CipherChild
		{
			public Cipher cipher;
			public Node child;
		}

		internal bool ValidateElementsInserted(List<int> expected, Func<Cipher, int> decode)
		{
			expected = expected.OrderBy(c => c).ToList();
			var actual = _root.GetAllCiphers().Select(c => decode(c)).OrderBy(c => c).ToList();

			return expected.Zip(actual, (a, b) => a == b).All(c => c);
		}
	}
}
