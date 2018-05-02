
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T, C, P>
	{
		private struct InsertInfo
		{
			public bool updated;
			public Node extraNode;

			public InsertInfo(bool updated = false, Node extraNode = null)
			{
				this.updated = updated;
				this.extraNode = extraNode;
			}
		}

		private struct DeleteInfo
		{
			/// <summary>
			/// True if the requested node is not found in the tree
			/// </summary>
			public bool notFound;

			/// <summary>
			/// If during the deletion process there is only one node left, it is returned
			/// Thus, root must be collapsed
			/// </summary>
			public Node onlyChild;

			/// <summary>
			/// If during the deletion process a merge ocurred, this value will contain 
			/// the orphan to be removed from the parent
			/// </summary>
			public Node orphan;

			public DeleteInfo(bool notFound = false, Node onlyChild = null, Node orphan = null)
			{
				this.notFound = notFound;
				this.onlyChild = onlyChild;
				this.orphan = orphan;
			}
		}

		private struct IndexValue
		{
			public C index;
			public Node node;

			public IndexValue(C index, Node node)
			{
				this.index = index;
				this.node = node;
			}
		}

		private abstract class Node
		{
			protected readonly Options<P, C> _options;
			protected List<IndexValue> children;

			/// <summary>
			/// Needed for deletion algorithm
			/// </summary>
			public Node next = null;
			public Node prev = null;
			public Node parent = null;

			private int _id;

			public Node(Options<P, C> options, Node parent, Node next, Node prev)
			{
				_options = options;

				this.parent = parent;
				this.next = next;
				this.prev = prev;

				Initialize();

				_id = options.GetNextId();
				
				options.OnVisit(this.GetHashCode());
			}

			/// <summary>
			/// Performs necessary operation to setup the tree
			/// before insertion of the first element
			/// </summary>
			protected virtual void Initialize()
			{
				children = new List<IndexValue>(_options.Branching + 2);
			}

			/// <summary>
			/// Returns the largest index among all children
			/// If there no children (during deletion process), smallest int is returned
			/// </summary>
			public virtual C LargestIndex()
			{
				_options.OnVisit(this.GetHashCode());

				return
					children.Count > 0 ?
					children.Select(ch => ch.index).Aggregate((acc, next) =>
					{
						acc = _options.Scheme.IsGreater(next, acc) ? next : acc;
						return acc;
					}) :
					_options.Scheme.MaxCiphertextValue();
			}

			/// <summary>
			/// Reflects the Tree method with the same name
			/// </summary>
			public virtual bool TryGet(C key, out T value)
			{
				_options.OnVisit(this.GetHashCode());

				value = default(T);

				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (_options.Scheme.IsLessOrEqual(key, children[i].index))
					{
						return children[i].node != null ? children[i].node.TryGet(key, out value) : false;
					}
				}

				// Should never be here
				return false;
			}

			/// <summary>
			/// Reflects the Tree method with the same name
			/// </summary>
			public virtual bool TryRange(C start, C end, List<T> values)
			{
				_options.OnVisit(this.GetHashCode());

				for (int i = 0; i < children.Count; i++)
				{
					if (_options.Scheme.IsLessOrEqual(start, children[i].index))
					{
						if (children[i].node == null)
						{
							return false;
						}

						return children[i].node.TryRange(start, end, values);
					}
				}

				// Should never be here
				return false;
			}

			/// <summary>
			/// Reflects the Tree method with the same name
			/// </summary>
			public abstract InsertInfo Insert(C key, T value);

			/// <summary>
			/// Reflects the Tree method with the same name
			/// </summary>
			/// <param name="key">Key to remove</param>
			/// <returns>The struct identifying the result of the inner delete</returns>
			public virtual DeleteInfo Delete(C key)
			{
				_options.OnVisit(this.GetHashCode());

				if (children.Count == 0)
				{
					return new DeleteInfo
					{
						notFound = true
					};
				}

				DeleteInfo result = new DeleteInfo();

				for (int i = 0; i < children.Count; i++)
				{
					if (_options.Scheme.IsLessOrEqual(key, children[i].index))
					{
						// last node and still not found
						if (children[i].node == null)
						{
							return new DeleteInfo
							{
								notFound = true
							};
						}

						result = children[i].node.Delete(key);
						break;
					}
				}

				if (result.notFound)
				{
					return new DeleteInfo
					{
						notFound = true
					};
				}

				// Merge ocurred
				if (result.orphan != null)
				{
					children.Remove(children.First(ch => ch.node == result.orphan));

					this.RebuildIndices(true);
				}

				if (children.Count == 0)
				{
					this.ConnectNeighbors();

					return new DeleteInfo
					{
						orphan = this
					};
				}

				// Underflow
				if (this.IsUnderflow())
				{
					// if this is root
					if (this.next == null && this.prev == null && this.parent == null)
					{
						if (children.Count <= 1)
						{
							return new DeleteInfo
							{
								onlyChild = children.First().node
							};
						}

						return new DeleteInfo();
					}

					// try to borrow from the right neighbor
					if (this.next != null)
					{
						var right = this.next.BorrowChildren(true);
						if (right != null)
						{
							children.AddRange(right);
							this.RebuildIndices(true);

							return new DeleteInfo();
						}
					}

					// try to borrow from the left neighbor
					if (this.prev != null)
					{
						var left = this.prev.BorrowChildren(false);
						if (left != null)
						{
							left.AddRange(children);
							children = left;
							this.RebuildIndices(true);

							return new DeleteInfo();
						}
					}

					// try to merge with the right neighbor
					if (this.next != null)
					{
						this.next.Merge(children, true);
						this.ConnectNeighbors();

						return new DeleteInfo
						{
							orphan = this
						};
					}

					// try to merge with the left neighbor
					// must be not null here
					if (this.prev != null)
					{
						this.prev.Merge(children, false);
						this.ConnectNeighbors();

						return new DeleteInfo
						{
							orphan = this
						};
					}

					// should never reach here
					throw new InvalidOperationException("Could not merge.");
				}

				return new DeleteInfo();
			}

			/// <summary>
			/// Merges into itself children of the neighbor node
			/// </summary>
			/// <param name="orphans">Children to adopt</param>
			/// <param name="fromLeft">True if the children come from the left side, false otherwise</param>
			protected void Merge(List<IndexValue> orphans, bool fromLeft)
			{
				_options.OnVisit(this.GetHashCode());

				if (fromLeft)
				{
					orphans.AddRange(children);
					children = orphans;
				}
				else
				{
					children.AddRange(orphans);
				}

				this.RebuildIndices(true);
			}

			/// <summary>
			/// Recursively (all the way up, if necessary) recomputes indices
			/// </summary>
			/// <param name="updateParent">Forcefully update the parent</param>
			protected void RebuildIndices(bool updateParent = false)
			{
				_options.OnVisit(this.GetHashCode());

				// leaf node
				if (children.Count == 0)
				{
					updateParent = true;
				}

				for (int i = 0; i < children.Count; i++)
				{
					// shadow infinity node
					if (children[i].node != null)
					{
						if (!_options.Scheme.IsEqual(children[i].index, children[i].node.LargestIndex()))
						{
							children[i] = new IndexValue
							{
								node = children[i].node,
								index = children[i].node.LargestIndex()
							};

							updateParent = true;
						}

						children[i].node.parent = this;
					}
				}

				if (updateParent && this.parent != null)
				{
					this.parent.RebuildIndices();
				}
			}

			/// <summary>
			/// Gives up part (half of the surplus) of own children to neighbor node
			/// If there are no children to spare, returns null
			/// </summary>
			/// <param name="leftMost">If true, children would be taken from the left side of the list</param>
			/// <returns>Spare children, or null</returns>
			protected List<IndexValue> BorrowChildren(bool leftMost)
			{
				_options.OnVisit(this.GetHashCode());

				var count = (children.Count - (_options.Branching / 2)) / 2;

				if (count == 0)
				{
					// Sorry, don't have spare children
					return null;
				}

				List<IndexValue> spareChildren;

				if (leftMost)
				{
					spareChildren = this.children.Take(count).ToList();
					children = children.Skip(count).Take(children.Count - count).ToList();
				}
				else
				{
					spareChildren = this.children.Skip(children.Count - count).Take(count).ToList();
					children = children.Take(children.Count - count).ToList();

				}

				// if rightmost children taken, it may affect parent's indices
				this.RebuildIndices(true);

				return spareChildren;
			}

			/// <summary>
			/// Returns true if for this particular node, underflow is detected
			/// Needed for proper deletion
			/// </summary>
			protected abstract bool IsUnderflow();

			/// <summary>
			/// Sets up next nad prev neighbors links
			/// Prepares itself for deletion
			/// </summary>
			protected void ConnectNeighbors()
			{
				if (this.prev != null)
				{
					this.prev.next = this.next;
				}

				if (this.next != null)
				{
					this.next.prev = this.prev;
				}
			}

			public virtual string ToString(int level, bool last, List<bool> nests, C index)
			{
				var result = "    ";

				for (int i = 0; i < level - 1; i++)
				{
					result += nests[i] ? "│   " : "    ";
				}

				result += $"{(last ? "└" : "├")}";

				result += $"─[<= { (_options.Scheme.IsEqual(index, _options.Scheme.MaxCiphertextValue()) ? "∞" : $"{index}").PadRight(3) }]";

				result += $"──({TypeString()} {children.Count}|{ Math.Round(100.0 * children.Count / _options.Branching) }%)\n";

				for (int i = 0; i < children.Count; i++)
				{
					if (children[i].node != null)
					{
						var isLast = i == children.Count - 1 || children[i + 1].node == null;

						var newNests = nests.ConvertAll(b => b);
						newNests.Add(!isLast);

						result += children[i].node.ToString(level + 1, isLast, newNests, children[i].index);
					}
				}

				return result;
			}

			/// <summary>
			/// Returns the character identifying the type of node
			/// Needed for better visualization
			/// </summary>
			public abstract string TypeString();

			/// <summary>
			/// Reflects the Tree method with the same name
			/// </summary>
			/// <param name="isRoot">Identify that this node is a root</param>
			/// <returns>Tru, if validation passes, false otherwise</returns>
			public virtual bool Validate(bool isRoot = false)
			{
				return true;
			}

			/// <summary>
			/// Verifies that the indices for this node and its children satisfy constraints
			/// </summary>
			/// <returns>True if check is passed, false otherwise</returns>
			public virtual bool CheckIndexes()
			{
				return
					children.Where(ch => ch.node != null).All(ch => _options.Scheme.IsEqual(ch.index, ch.node.LargestIndex())) &&
					children.Where(ch => ch.node != null).All(ch => ch.node.CheckIndexes());
			}

			/// <summary>
			/// Verifies that the links (next, prev, parent) for this node and its siblings and its children 
			/// satisfy the constraints
			/// </summary>
			/// <param name="leftMost">If true, than this node is the "first" of its siblings</param>
			/// <param name="isRoot">True, if this node is a root</param>
			/// <returns>True, if check is passed, false otherwise</returns>
			public virtual bool CheckNeighborLinks(bool leftMost = false, bool isRoot = false)
			{
				var thisNextLink = (this.next != null || _options.Scheme.IsEqual(this.children.Last().index, _options.Scheme.MaxCiphertextValue()));
				var siblingsChain = (this.next == null || this.next.CheckNeighborLinks());
				var thisPrevLink = (leftMost == (this.prev == null));
				var thisParentLink = (isRoot == (this.parent == null));
				var childrenChain = (leftMost ? this.children.First().node.CheckNeighborLinks(true) : true);
				var nextPrevLink = (this.next == null || this.next.prev == this);
				var prevNextLink = (this.prev == null || this.prev.next == this);

				var result =
					thisNextLink &&
					siblingsChain &&
					thisPrevLink &&
					thisParentLink &&
					childrenChain &&
					nextPrevLink &&
					prevNextLink;

				return result;
			}

			/// <summary>
			/// Returns the maximum height of all of its children
			/// </summary>
			protected virtual int Height()
			{
				return 1 + children
					.Where(ch => ch.node != null)
					.Max(ch => ch.node.Height());
			}

			/// <summary>
			/// Verifies that the tree is balanced;
			/// that is, all paths are of equal lengths
			/// </summary>
			/// <returns>True, if check is passed, false otherwise</returns>
			public virtual bool isBalanced()
			{
				return
					children
						.Where(ch => ch.node != null)
						.Select(ch => ch.node.Height())
						.Distinct()
						.Count() == 1 &&
					children
						.Where(ch => ch.node != null)
						.All(ch => ch.node.isBalanced());
			}

			public override int GetHashCode() => _id;
		}
	}
}
