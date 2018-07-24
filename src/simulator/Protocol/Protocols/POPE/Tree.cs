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
			LeafNode Split(C point)
			{
				SplitResult split = _root.Split(point, default(C));
				var newRoot = split.leaf?.parent.Rebalance();

				if (newRoot != null)
				{
					_root = newRoot;
				}
				else if (split.newRoot != null)
				{
					_root = split.newRoot;
				}

				return split.leaf;
			}

			LeafNode leftLeaf = Split(left);
			LeafNode rightLeaf = Split(right);

			List<string> result = new List<string>();

			do
			{
				result.AddRange(leftLeaf.RetrieveBuffer());
				// A bug in original POPE paper
				// If item is in intermediate node between endpoints 
				// it is not included in the response
				// Here is the fix
				var parent = leftLeaf.parent;
				while (parent != null)
				{
					result.AddRange(parent.RetrieveBuffer());
					parent = parent.parent;
				}

				if (leftLeaf == rightLeaf)
				{
					break;
				}
				leftLeaf = (LeafNode)leftLeaf.right;
			} while (true);

			return result;
		}

		internal void Validate(Func<C, long> decode) => _root.Validate(decode, long.MinValue, long.MaxValue);

		private abstract class Node
		{
			public InternalNode parent = null;

			protected readonly Options<C> _options;

			protected readonly HashSet<EncryptedRecord<C>> _buffer = new HashSet<EncryptedRecord<C>>();

			public Node(Options<C> options)
			{
				_options = options;
			}

			public void Insert(EncryptedRecord<C> block) => _buffer.Add(block);

			public abstract SplitResult Split(C label, C max, SplitResult split = null);

			public void AcceptChild(EncryptedRecord<C> child) => _buffer.Add(child);

			public List<string> RetrieveBuffer() => _buffer.Select(b => b.value).ToList();

			internal abstract List<C> GetAllCiphers();

			internal abstract void Validate(Func<C, long> decode, long from, long to);
		}

		private class InternalNode : Node
		{
			private readonly List<CipherChild> _children = new List<CipherChild>();

			public InternalNode(Options<C> options) : base(options) { }

			public InternalNode(Options<C> options, List<CipherChild> children) : base(options)
			{
				_children = children;
			}

			public InternalNode Rebalance(InternalNode newRoot = null)
			{
				if (_children.Count <= _options.L)
				{
					return newRoot;
				}

				InternalNode root;
				if (parent == null)
				{
					root = new InternalNode(_options);
					newRoot = root;
				}
				else
				{
					root = parent;
				}

				var partitions = _children.InSetsOf(_options.L).ToList();

				root.AcceptInternals(this, partitions);

				return root.Rebalance(newRoot);
			}

			public void AcceptInternals(InternalNode child, List<List<CipherChild>> partitions)
			{
				var toInsert = partitions
					.Select(p =>
					{
						var result = new CipherChild
						{
							child = new InternalNode(_options, p),
							cipher = p.Last().cipher
						};
						p.ToList().ForEach(c => c.child.parent = (InternalNode)result.child);
						return result;
					})
					.ToList();

				toInsert.ForEach(c => c.child.parent = this);

				if (_children.Count > 0)
				{
					for (int i = 0; i < _children.Count; i++)
					{
						if (_children[i].child == child)
						{
							var thisChild = _children[i].child;

							_children.RemoveAt(i);
							_children.InsertRange(i, toInsert);

							break;
						}
					}
				}
				else
				{
					_children.AddRange(toInsert);
				}
			}

			public override SplitResult Split(C label, C max, SplitResult split = null)
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

					result = _children[resultIndex].child.Split(label, _children[resultIndex].cipher);
				}
				else
				{
					result = split;
				}

				while (result.wasSplit)
				{
					CipherChild pair = AcceptChildren(result.child, result.buffer, result.list, label);
					result = pair.child.Split(label, pair.cipher);
				}

				result.newRoot = result.newRoot ?? split?.newRoot;

				return result;
			}

			public CipherChild AcceptChildren(LeafNode child, HashSet<EncryptedRecord<C>> buffer, List<C> list, C label)
			{
				// if (_children.Count == 0)
				// {
				// 	list.Add(default(C));
				// }

				var toInsert = list.Select(c => new CipherChild { cipher = c, child = new LeafNode(_options) }).ToList();
				for (int i = 0; i < toInsert.Count; i++)
				{
					if (i != 0)
					{
						((LeafNode)toInsert[i].child).left = (LeafNode)toInsert[i - 1].child;
					}
					if (i != toInsert.Count - 1)
					{
						((LeafNode)toInsert[i].child).right = (LeafNode)toInsert[i + 1].child;
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
							LeafNode thisChild = (LeafNode)_children[i].child;

							if (thisChild.left != null)
							{
								thisChild.left.right = (LeafNode)toInsert.First().child;
								((LeafNode)toInsert.First().child).left = thisChild.left;
							}

							if (thisChild.right != null)
							{
								thisChild.right.left = (LeafNode)toInsert.Last().child;
								((LeafNode)toInsert.Last().child).right = thisChild.right;
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

				return _children[resultIndex];
			}

			internal override List<C> GetAllCiphers() =>
				_buffer.Select(b => b.cipher).Concat(_children.Select(c => c.child.GetAllCiphers()).SelectMany(c => c)).ToList();

			internal override void Validate(Func<C, long> decode, long from, long to)
			{
				// number of children
				if (_children.Count > _options.L)
				{
					throw new InvalidOperationException("Children count");
				}

				// children bounds
				if (decode(_children.First().cipher) <= from)
				{
					throw new InvalidOperationException("Children lower bound");
				}
				if (decode(_children.Last().cipher) > to)
				{
					throw new InvalidOperationException("Children upper bound");
				}

				// children in order
				for (int i = 0; i < _children.Count - 1; i++)
				{
					if (decode(_children[i].cipher) >= decode(_children[i + 1].cipher))
					{
						throw new InvalidOperationException("Children order");
					}
				}

				// has parent
				if ((!(from == long.MinValue && to == long.MaxValue)) && parent == null)
				{
					throw new InvalidOperationException("Parent unset");
				}

				// validate children
				for (int i = 0; i < _children.Count; i++)
				{
					long lower;
					if (i == 0)
					{
						lower = long.MinValue;
					}
					else
					{
						lower = decode(_children[i - 1].cipher);
					}

					if (_children[i].child.parent != this)
					{
						throw new InvalidOperationException("Child parent relationship");
					}

					_children[i].child.Validate(decode, lower, decode(_children[i].cipher));
				}
			}
		}

		private class LeafNode : Node
		{
			public LeafNode right = null;
			public LeafNode left = null;

			public LeafNode(Options<C> options) : base(options)
			{
			}

			public override SplitResult Split(C label, C max, SplitResult split = null)
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

				if (!labels.Contains(max))
				{
					labels.Remove(labels.ElementAt(_options.G.Next(0, labels.Count - 1)));
					labels.Add(max);
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
					return newRoot.Split(label, default(C), result);
				}
			}

			internal override List<C> GetAllCiphers() => _buffer.Select(b => b.cipher).ToList();

			internal override void Validate(Func<C, long> decode, long from, long to)
			{
				// children bounds
				if (_buffer.Count > 0)
				{
					if (_buffer.Min(c => decode(c.cipher)) <= from)
					{
						throw new InvalidOperationException("Buffer lower bound");
					}
					if (_buffer.Max(c => decode(c.cipher)) > to)
					{
						throw new InvalidOperationException("Buffer upper bound");
					}
				}
				else
				{
					if (to != long.MaxValue)
					{
						throw new InvalidOperationException("Buffer empty for not last child");
					}
				}

				// sibling links
				if (from != long.MinValue && left == null)
				{
					throw new InvalidOperationException("Left unset");
				}

				if (to != long.MaxValue && right == null)
				{
					throw new InvalidOperationException("Right unset");
				}

				// has parent
				if ((!(from == long.MinValue && to == long.MaxValue)) && parent == null)
				{
					throw new InvalidOperationException("Parent unset");
				}
			}
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

		internal bool ValidateElementsInserted(List<long> expected, Func<C, long> decode)
		{
			expected = expected.OrderBy(c => c).ToList();
			var actual = _root.GetAllCiphers().Select(c => decode(c)).OrderBy(c => c).ToList();

			return expected.Zip(actual, (a, b) => a == b).All(c => c);
		}
	}
}
