using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class LeafNode : Node
		{
			private LeafNode next = null;

			public LeafNode(Options options) : base(options) { }

			public LeafNode(Options options, LeafNode next, List<IndexValue> children) : base(options)
			{
				this.next = next;
				this.children = children;
			}

			public override bool TryRange(int start, int end, List<T> values)
			{
				var found = false;

				for (int i = 0; i < children.Count; i++)
				{
					if (start <= children[i].index && end >= children[i].index && children[i].node != null)
					{
						found = true;

						T value;
						children[i].node.TryGet(children[i].index, out value);
						values.Add(value);
					}
				}

				return (next == null ? false : next.TryRange(Int32.MinValue, end, values)) || found;
			}

			public override Node Insert(int key, T value)
			{
				if (children.Count == 0)
				{
					children.Add(new IndexValue(key, new DataNode(_options, key, value)));
					children.Add(new IndexValue(Int32.MaxValue, null));

					return null;
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (children[i].node == null)
					{
						new DataNode(_options, key, value);
					}

					if (key <= children[i].index)
					{
						// Update then
						if (key == children[i].index)
						{
							children[i].node.Insert(key, value);
						}
						else
						{
							children.Insert(i, new IndexValue(key, new DataNode(_options, key, value)));
						}

						break;
					}
				}

				if (children.Count > _options.Branching)
				{
					// Split
					var half = children.Count / 2 + children.Count % 2;

					var newNodeChildren = this.children.Skip(half).ToList();
					var newNode = new LeafNode(_options, this.next, newNodeChildren);
					next = newNode;

					children = children.Take(half).ToList();

					return newNode;
				}

				return null;
			}

			public override string TypeString()
			{
				return "L";
			}

			public override void Validate(bool isRoot)
			{
				bool atLeastOneChild = children.Count > 0;
				bool nextDefined = next != null || children.Last().index == Int32.MaxValue; // Rightmost leaf
				bool childrenOrdered =
					children
						.Zip(
							children.Skip(1),
							(a, b) => new { a, b }
						)
						.All(pair => pair.a.index < pair.b.index);

				if (
					!atLeastOneChild ||
					!nextDefined ||
					!childrenOrdered
				)
				{
					throw new InvalidOperationException("Leaf node is not valid");
				}
			}
		}

	}
}
