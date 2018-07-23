using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.POPE
{
	internal class Options<C> where C : IGetSize
	{
		public int L;
		public Action<HashSet<C>> SetList;
		public Func<List<C>> GetSortedList;
		public Func<C, int> IndexToInsert;
		public Func<C, int> IndexOfResult;
		public IPRG G;
	}

	internal class Tree<C> where C : IGetSize
	{
		private Node _root;

		public Tree(Options<C> options)
		{
			_root = new LeafNode(options);
		}

		public void Insert(EncryptedRecord<C> block) => _root.Insert(block);

		public List<string> Search(C left, C right)
		{
			SplitResult leftSplit = _root.Split(left);
			if (leftSplit.newRoot != null && leftSplit.newRoot != _root)
			{
				_root = leftSplit.newRoot;
			}

			SplitResult rightSplit = _root.Split(right);
			if (rightSplit.newRoot != null && rightSplit.newRoot != _root)
			{
				_root = rightSplit.newRoot;
			}

			List<string> result = new List<string>();

			LeafNode leftLeaf = leftSplit.leaf;

			do
			{
				result.AddRange(leftLeaf._buffer.Select(b => b.value));
				leftLeaf = (LeafNode)leftLeaf.right;
			} while (leftLeaf != rightSplit.leaf);

			result.AddRange(rightSplit.leaf._buffer.Select(b => b.value));

			return result;
		}

		private abstract class Node
		{
			public Node parent = null;
			public Node right = null;
			public Node left = null;

			protected readonly Options<C> _options;

			public Node(Options<C> options)
			{
				_options = options;
			}

			public readonly HashSet<EncryptedRecord<C>> _buffer = new HashSet<EncryptedRecord<C>>();

			public void Insert(EncryptedRecord<C> block) => _buffer.Add(block);

			public abstract SplitResult Split(C label, SplitResult split = null);

			public void AcceptChild(EncryptedRecord<C> child) => _buffer.Add(child);

			internal abstract List<C> GetAllCiphers();
		}

		private class InternalNode : Node
		{
			private readonly List<CipherChild> _children = new List<CipherChild>();

			public InternalNode(Options<C> options) : base(options) { }

			public override SplitResult Split(C label, SplitResult split = null)
			{
				SplitResult result;

				if (split == null)
				{
					_options.SetList(new HashSet<C>(_children.Select(c => c.cipher)));

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

					result = _children[resultIndex].child.Split(label);
				}
				else
				{
					result = split;
				}

				while (result.wasSplit)
				{
					LeafNode child = AcceptChildren(result.child, result.buffer, result.list, label);
					result = child.Split(label);
				}

				result.newRoot = result.newRoot ?? split?.newRoot;

				return result;
			}

			public LeafNode AcceptChildren(LeafNode child, HashSet<EncryptedRecord<C>> buffer, List<C> list, C label)
			{
				if (_children.Count == 0)
				{
					list.Add(default(C));
				}

				var toInsert = list.Select(c => new CipherChild { cipher = c, child = new LeafNode(_options) }).ToList();
				for (int i = 0; i < toInsert.Count; i++)
				{
					if (i != 0)
					{
						toInsert[i].child.left = toInsert[i - 1].child;
					}
					if (i != toInsert.Count - 1)
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
								toInsert.First().child.left = thisChild.left;
							}

							if (thisChild.right != null)
							{
								thisChild.right.left = toInsert.Last().child;
								toInsert.Last().child.right = thisChild.right;
							}

							// we must always have an upper value
							if (_children[i].cipher == null)
							{
								toInsert.Last().cipher = default(C);
							}

							_children.RemoveAt(i);
							_children.InsertRange(i, toInsert);

							break;
						}
					}
				}

				_options.SetList(new HashSet<C>(_children.Select(c => c.cipher)));

				foreach (var block in buffer)
				{
					var index = _options.IndexToInsert(block.cipher);
					_children[index].child.AcceptChild(block);
				}

				var resultIndex = _options.IndexOfResult(label);

				return (LeafNode)_children[resultIndex].child;
			}

			internal override List<C> GetAllCiphers() =>
				_children.Select(c => c.cipher).Concat(_buffer.Select(b => b.cipher)).Concat(_children.Select(c => c.child.GetAllCiphers()).SelectMany(c => c)).ToList();
		}

		private class LeafNode : Node
		{
			public LeafNode(Options<C> options) : base(options)
			{
			}

			public override SplitResult Split(C label, SplitResult split = null)
			{
				SplitResult result = new SplitResult();

				if (_buffer.Count <= _options.L)
				{
					result.leaf = this;
					return result;
				}

				HashSet<C> labels = new HashSet<C>();

				for (int i = 0; i < _options.L; i++)
				{
					var sampled = _buffer.ElementAt(_options.G.Next(0, _buffer.Count - 1));
					if (labels.Contains(sampled.cipher))
					{
						i--;
						continue;
					}
					labels.Add(sampled.cipher);
				}

				_options.SetList(labels);

				InternalNode newRoot;

				if (parent == null)
				{
					newRoot = new InternalNode(_options);
					result.newRoot = newRoot;
				}
				else
				{
					newRoot = (InternalNode)parent;
				}

				var sorted = _options.GetSortedList();

				result.wasSplit = true;
				result.buffer = _buffer;
				result.list = sorted;
				result.child = this;

				if (result.newRoot == null)
				{
					return result;
				}
				else
				{
					return newRoot.Split(label, result);
				}
			}

			internal override List<C> GetAllCiphers() => _buffer.Select(b => b.cipher).ToList();
		}

		private class CipherChild
		{
			public C cipher;
			public Node child;
		}

		private class SplitResult
		{
			public LeafNode leaf;
			public InternalNode newRoot;

			public bool wasSplit = false;
			public HashSet<EncryptedRecord<C>> buffer;
			public List<C> list;
			public LeafNode child;
		}

		internal bool ValidateElementsInserted(List<int> expected, Func<C, int> decode)
		{
			expected = expected.OrderBy(c => c).ToList();
			var actual = _root.GetAllCiphers().Select(c => decode(c)).OrderBy(c => c).ToList();

			return expected.Zip(actual, (a, b) => a == b).All(c => c);
		}
	}
}
