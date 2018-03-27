using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class InternalNode : Node
		{
			public InternalNode(Options options, Node parent, Node next, Node prev) : base(options, parent, next, prev) { }

			public InternalNode(Options options, Node parent, Node next, Node prev, List<IndexValue> children) : base(options, parent, next, prev)
			{
				this.children = children;
				this.children.Where(ch => ch.node != null).ToList().ForEach(ch => ch.node.SetParent(this));
			}

			public override Node Insert(int key, T value)
			{
				Node extraNode = null;
				Node prevNode = null;
				int insertedIndex = -1;

				for (int i = 0; i < children.Count; i++)
				{
					if (key <= children[i].index)
					{
						insertedIndex = i;
						prevNode = children[i].node;
						extraNode = children[i].node.Insert(key, value);

						break;
					}
				}

				if (extraNode == null)
				{
					return null;
				}

				var newKey = prevNode.LargestIndex();

				children[insertedIndex] = new IndexValue(children[insertedIndex].index, extraNode);

				children.Insert(insertedIndex, new IndexValue(newKey, prevNode));

				if (children.Count > _options.Branching)
				{
					// Split
					var half = children.Count / 2 + children.Count % 2;

					var newNodeChildren = this.children.Skip(half).ToList();
					var newNode = new InternalNode(_options, this.parent, this.next, this, newNodeChildren);
					this.next = newNode;

					children = children.Take(half).ToList();

					return newNode;
				}

				return null;
			}

			public override string TypeString()
			{
				return "I";
			}

			public override void Validate(bool isRoot)
			{
				bool underflow =
					children.Count < 0.5 * _options.Branching && !isRoot ||
					children.Count < 2 && isRoot;
				bool overflow = children.Count > _options.Branching;
				bool childrenOrdered =
					children
						.Zip(
							children.Skip(1),
							(a, b) => new { a, b }
						)
						.All(pair => pair.a.index < pair.b.index);

				if (
					!childrenOrdered ||
					underflow ||
					overflow
				)
				{
					throw new InvalidOperationException("Internal node is not valid");
				}

				children.ForEach(ch => ch.node.Validate());
			}

			protected override bool IsUnderflow()
			{
				return children.Count < (_options.Branching / 2) + (_options.Branching % 2);
			}
		}
	}
}
