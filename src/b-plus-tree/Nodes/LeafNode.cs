using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class LeafNode : Node
		{
			public LeafNode next = null;

			public LeafNode(Options options) : base(options) { }

			public LeafNode(Options options, LeafNode next, List<IndexValue> children) : base(options)
			{
				this.next = next;
				this.children = children;
			}

			public override bool TryRange(int start, int end, List<T> values = null)
			{
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

						T value;
						children[i].node.TryGet(children[i].index, out value);
						values.Add(value);

						next.TryRange(Int32.MinValue, end, values);

						return true;
					}
				}

				return false;
			}

			public override Node Insert(int key, T value)
			{
				if (children.Count == 0)
				{
					children.Add(new IndexValue(key, new DataNode(_options, value)));
					children.Add(new IndexValue(Int32.MaxValue, null));

					return null;
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (children[i].node == null)
					{
						new DataNode(_options, value);
					}

					if (key <= children[i].index)
					{
						children.Insert(i, new IndexValue(key, new DataNode(_options, value)));

						break;
					}
				}

				if (children.Count > _options.Branching)
				{
					// Split
					var half = children.Count / 2 + children.Count % 2;

					var newNodeChildren = this.children.Skip(half).ToList();
					// newNodeChildren.Insert(0, new IndexValue(Int32.MinValue, null));
					var newNode = new LeafNode(_options, this.next, newNodeChildren);

					children = children.Take(half).ToList();
					// children.Add(new IndexValue(Int32.MaxValue, null));

					return newNode;
				}

				return null;
			}

			public override string TypeString()
			{
				return "L";
			}
		}

	}
}
