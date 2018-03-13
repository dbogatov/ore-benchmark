
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{
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

			public Node(Options options)
			{
				_options = options;

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

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (key >= children[i].index && key <= children[i + 1].index)
					{
						if (children[i].node == null)
						{
							return false;
						}

						return children[i].node.TryGet(key, out value);
					}
				}

				return false;
			}

			public virtual bool TryRange(int start, int end, List<T> values = null)
			{
				if (start >= end)
				{
					throw new ArgumentException("Improper range");
				}

				if (values == null)
				{
					values = new List<T>();
				}

				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (
						start >= children[i].index && start <= children[i + 1].index ||
						children[i].index >= start && children[i].index <= end
					)
					{
						if (children[i].node == null)
						{
							return false;
						}

						return children[i].node.TryRange(start, end, values);
					}
				}

				return false;
			}

			public virtual Node Insert(int key, T value)
			{
				throw new NotImplementedException("Should never be called on this instance");
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
