using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T, C, P>
	{

		private class InternalNode : Node
		{
			public InternalNode(Options<P, C> options, Node parent, Node next, Node prev) : base(options, parent, next, prev) { }

			public InternalNode(Options<P, C> options, Node parent, Node next, Node prev, List<IndexValue> children) : base(options, parent, next, prev)
			{
				this.children = children;
				this.children.Where(ch => ch.node != null).ToList().ForEach(ch => ch.node.parent = this);
			}

			public override InsertInfo Insert(C key, T value)
			{
				_options.OnVisit(this.GetHashCode());
				
				Node extraNode = null;
				Node prevNode = null;
				int insertedIndex = -1;
				bool updated = false;

				for (int i = 0; i < children.Count; i++)
				{
					if (_options.Scheme.IsLessOrEqual(key, children[i].index))
					{
						insertedIndex = i;
						prevNode = children[i].node;
						var result = children[i].node.Insert(key, value);
						extraNode = result.extraNode;
						updated = result.updated;

						break;
					}
				}

				if (extraNode == null)
				{
					return new InsertInfo
					{
						updated = updated
					};
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

					if (this.next != null)
					{
						this.next.prev = newNode;
					}
					this.next = newNode;

					children = children.Take(half).ToList();

					return new InsertInfo
					{
						updated = updated,
						extraNode = newNode
					};
				}

				return new InsertInfo
				{
					updated = updated
				};
			}

			public override string TypeString()
			{
				return "I";
			}

			public override bool Validate(bool isRoot)
			{
				bool underflow =
					children.Count < (_options.Branching / 2) + (_options.Branching % 2) && !isRoot ||
					children.Count < 2 && isRoot;
				bool overflow = children.Count > _options.Branching;
				bool childrenOrdered =
					children
						.Zip(
							children.Skip(1),
							(a, b) => new { a, b }
						)
						.All(pair => _options.Scheme.IsLess(pair.a.index, pair.b.index));

				return
					childrenOrdered &&
					!underflow &&
					!overflow &&
					children.All(ch => ch.node.Validate());
			}

			protected override bool IsUnderflow()
			{
				return children.Count < (_options.Branching / 2) + (_options.Branching % 2);
			}
		}
	}
}
