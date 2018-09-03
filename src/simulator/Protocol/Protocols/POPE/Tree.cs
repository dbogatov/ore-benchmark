using System;
using System.Collections.Generic;
using System.Linq;
using BPlusTree;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.POPE
{
	/// <summary>
	/// Data structure holding configuration for the tree
	/// Actions and functions are a way for tree interact with "client"
	/// </summary>
	/// <typeparam name="C">Ciphertext type</typeparam>
	internal class Options<C> where C : IGetSize
	{
		public virtual event NodeVisitedEventHandler NodeVisited;

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

		private Random _generator = new Random(123456);

		/// <summary>
		/// Returns the next unique available id for a node
		/// </summary>s
		public int GetNextId() => _generator.Next();

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

		/// <summary>
		/// Adds an item to the tree
		/// </summary>
		/// <param name="block">A block to add</param>
		public void Insert(EncryptedRecord<C> block) => _root.Insert(block);

		/// <summary>
		/// Performs a range query on tree
		/// </summary>
		/// <param name="left">Left endpoint</param>
		/// <param name="right">Right endpoint</param>
		/// <returns>A list of strings as a result of search</returns>
		/// <remarks>
		/// May return a superset of the real answer
		/// </remarks>
		public List<string> Search(C left, C right)
		{
			LeafNode Split(C point)
			{
				SplitResult split = _root.Split(point, default(C));
				var newRoot = split.leaf.parent?.Rebalance();

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

		/// <summary>
		/// Internal function for debug purposes.
		/// Verifies that tree structure is valid.
		/// </summary>
		/// <param name="decode">A function to decode a ciphertext into orderable value</param>
		internal void Validate(Func<C, long> decode) => _root.Validate(decode, long.MinValue, long.MaxValue);

		private abstract class Node
		{
			public InternalNode parent = null;

			protected readonly Options<C> _options;

			protected readonly HashSet<EncryptedRecord<C>> _buffer = new HashSet<EncryptedRecord<C>>();

			private int _id;

			public Node(Options<C> options)
			{
				_options = options;

				_id = options.GetNextId();

				ReportNodeVisited();
			}

			/// <summary>
			/// Mirrors corresponding Tree method
			/// </summary>
			public void Insert(EncryptedRecord<C> block)
			{
				ReportNodeVisited(justAppend: true);

				_buffer.Add(block);
			}

			/// <summary>
			/// Splits a node if necessary (as described in the original paper)
			/// </summary>
			/// <param name="label">Ciphertext to search for</param>
			/// <param name="max">Greatest value the the node is supposed to have (needed when sampling children)</param>
			/// <param name="split">If this node was created as a result of child's split, this is the result of that split</param>
			/// <returns>The result of the split (complex object)</returns>
			public abstract SplitResult Split(C label, C max, SplitResult split = null);

			/// <summary>
			/// Makes node add a child to itself (buffer)
			/// </summary>
			/// <param name="child">A child to insert</param>
			public void AcceptChild(EncryptedRecord<C> child) => _buffer.Add(child);

			/// <summary>
			/// Returns values from its buffer
			/// </summary>
			public List<string> RetrieveBuffer()
			{
				ReportNodeVisited();

				return _buffer.Select(b => b.value).ToList();
			}

			/// <summary>
			/// Internal function for debug purposes.
			/// Returns its ciphertexts and children's ciphertexts.
			/// </summary>
			internal abstract List<C> GetAllCiphers();

			/// <summary>
			/// Mirrors corresponding Tree method
			/// </summary>
			/// <param name="decode">Function that converts ciphers to orderable numbers</param>
			/// <param name="from">Smallest eligible ciphers decryption for this node</param>
			/// <param name="to">Largest eligible ciphers decryption for this node</param>
			internal abstract void Validate(Func<C, long> decode, long from, long to);

			public override int GetHashCode() => _id;

			protected void ReportNodeVisited(bool justAppend = false)
			{
				var elements = ElementsNumber();

				if (justAppend)
				{
					// If we only append we count only the last block 
					_options.OnVisit(this.GetHashCode() * 2 + (elements / (_options.L + 1)) * 3);
					return;
				}

				for (int i = 0; i <= elements / (_options.L + 1); i++)
				{
					_options.OnVisit(this.GetHashCode() * 2 + i * 3);
				}
			}

			protected abstract int ElementsNumber();
		}

		private class InternalNode : Node
		{
			private readonly List<CipherChild> _children = new List<CipherChild>();

			public InternalNode(Options<C> options) : base(options) { }

			public InternalNode(Options<C> options, List<CipherChild> children) : base(options)
			{
				_children = children;
			}

			/// <summary>
			/// Splits this node if it has too many children
			/// </summary>
			/// <param name="newRoot">If new root was created in a recursive call, this node is here</param>
			/// <returns>A new root, if it was created</returns>
			public InternalNode Rebalance(InternalNode newRoot = null)
			{
				ReportNodeVisited();

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

			/// <summary>
			/// Makes this node accept new children and insert them in its list properly
			/// </summary>
			/// <param name="child">Child that caused the acceptance and will be removed</param>
			/// <param name="partitions">Sets of children to insert (this node picks largest from each set)</param>
			public void AcceptInternals(InternalNode child, List<List<CipherChild>> partitions)
			{
				ReportNodeVisited();

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
				ReportNodeVisited();

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

			/// <summary>
			/// Makes this node accept new leaf nodes as children
			/// </summary>
			/// <param name="child">Child that caused an acceptance and will be removed</param>
			/// <param name="buffer">Set of encrypted records to distribute among children</param>
			/// <param name="list">List of children to accept</param>
			/// <param name="label">Ciphertext to search for</param>
			/// <returns>Child that contans requested ciphertext</returns>
			public CipherChild AcceptChildren(LeafNode child, HashSet<EncryptedRecord<C>> buffer, List<C> list, C label)
			{
				ReportNodeVisited();

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

			protected override int ElementsNumber() => _buffer.Count + _children.Count;
		}

		private class LeafNode : Node
		{
			public LeafNode right = null;
			public LeafNode left = null;

			public LeafNode(Options<C> options) : base(options) { }

			public override SplitResult Split(C label, C max, SplitResult split = null)
			{
				ReportNodeVisited();

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

			protected override int ElementsNumber() => _buffer.Count;
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

		/// <summary>
		/// Verifies that all supplied elements are in the tree
		/// </summary>
		/// <param name="expected">Suplied elements</param>
		/// <param name="decode">Function to get orderable value from cipher</param>
		/// <returns>True if check passed</returns>
		internal bool ValidateElementsInserted(List<long> expected, Func<C, long> decode)
		{
			expected = expected.OrderBy(c => c).ToList();
			var actual = _root.GetAllCiphers().Select(c => decode(c)).OrderBy(c => c).ToList();

			return expected.Zip(actual, (a, b) => a == b).All(c => c);
		}
	}
}
