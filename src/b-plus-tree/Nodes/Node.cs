
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{
		private struct DeleteInfo
		{
			/// <summary>
			/// True if the requested node is not found in the tree
			/// </summary>
			public bool notFound;

			/// <summary>
			/// True if during the deletion process a merge or a shift ocurred
			/// Thus, it is necessary to rebuild indices
			/// </summary>
			public bool mergeOrShiftOcurred;

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

			public DeleteInfo(bool notFound = false, Node onlyChild = null, bool mergeOrShiftOcurred = false, Node orphan = null)
			{
				this.notFound = notFound;
				this.onlyChild = onlyChild;
				this.mergeOrShiftOcurred = mergeOrShiftOcurred;
				this.orphan = orphan;
			}
		}

		private struct IndexValue
		{
			public int index;
			public Node node;

			public IndexValue(int index, Node node)
			{
				this.index = index;
				this.node = node;
			}
		}

		private abstract class Node
		{
			protected readonly Options _options;
			protected List<IndexValue> children;

			/// <summary>
			/// Needed for deletion algorithm
			/// </summary>
			protected Node next = null;
			protected Node prev = null;
			protected Node parent = null;

			public Node(Options options, Node parent, Node next, Node prev)
			{
				_options = options;

				this.parent = parent;
				this.next = next;
				this.prev = prev;

				Initialize();
			}

			protected virtual void Initialize()
			{
				children = new List<IndexValue>(_options.Branching + 2);
			}

			public int LargestIndex()
			{
				return children.Max(ch => ch.index);
			}

			public virtual bool TryGet(int key, out T value)
			{
				value = default(T);

				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (key <= children[i].index)
					{
						return children[i].node.TryGet(key, out value);
					}
				}

				// Should never be here
				return false;
			}

			public virtual bool TryRange(int start, int end, List<T> values)
			{
				for (int i = 0; i < children.Count; i++)
				{
					if (start <= children[i].index)
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

			public abstract Node Insert(int key, T value);

			public virtual DeleteInfo Delete(int key, bool isRoot = false)
			{
				// TODO: check if this case is even possible
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
					if (key <= children[i].index)
					{
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

				if (result.mergeOrShiftOcurred)
				{
					// Merge ocurred
					if (result.orphan != null)
					{
						children.Remove(children.First(ch => ch.node == result.orphan));
					}

					// Rebuild indices
					for (int i = 0; i < children.Count; i++)
					{
						children[i] = new IndexValue
						{
							node = children[i].node,
							index = children[i].node.LargestIndex()
						};
					}

					// Underflow
					if (this.IsUnderflow())
					{
						// if this is root
						if (this.next == null && this.prev == null)
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

						if (this.next != null)
						{
							var fromRight = this.next.BorrowChildren(true);
							if (fromRight != null)
							{

							}
						}
					}
				}

				return new DeleteInfo();
			}

			protected List<IndexValue> BorrowChildren(bool leftMost)
			{
				var count = (children.Count - (_options.Branching / 2)) / 2;

				if (count == 0)
				{
					// Sorry, don't have spare children
					return null;
				}

				if (leftMost)
				{
					var spareChildren = this.children.Take(count).ToList();
					children = children.Skip(count).Take(children.Count - count).ToList();

					return spareChildren;
				}
				else
				{
					var spareChildren = this.children.Skip(children.Count - count).Take(count).ToList();
					children = children.Take(children.Count - count).ToList();

					return spareChildren;
				}

			}

			protected abstract bool IsUnderflow();

			public void SetParent(Node parent)
			{
				this.parent = parent;
			}

			public virtual string ToString(int level, bool last, List<bool> nests, int index)
			{
				var result = "    ";

				for (int i = 0; i < level - 1; i++)
				{
					result += nests[i] ? "│   " : "    ";
				}

				result += $"{(last ? "└" : "├")}";

				if (index != Int32.MinValue)
				{
					result += $"─[<= { (index == Int32.MaxValue ? "∞" : $"{index}").PadRight(3) }]";
				}

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

			public abstract string TypeString();

			public virtual void Validate(bool isRoot = false) { }

			public virtual bool CheckNeighborLinks(bool leftMost = false)
			{
				return 
					(this.next != null || this.children.Last().index == Int32.MaxValue) &&
					(this.next == null || this.next.CheckNeighborLinks()) &&
					(leftMost ? this.children.First().node.CheckNeighborLinks(true) : true);
			}

			protected virtual int Height()
			{
				return 1 + children
					.Where(ch => ch.node != null)
					.Max(ch => ch.node.Height());
			}

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
		}
	}
}
