using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class LeafNode : Node
		{
			public LeafNode(Options options, Node parent, Node next, Node prev) : base(options, parent, next, prev) { }

			public LeafNode(Options options, Node parent, Node next, Node prev, List<IndexValue> children) : base(options, parent, next, prev)
			{
				this.children = children;
				this.children.Where(ch => ch.node != null).ToList().ForEach(ch => ch.node.parent = this);
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

			public override InsertInfo Insert(int key, T value)
			{
				var updated = false;

				if (children.Count == 0)
				{
					children.Add(new IndexValue(key, new DataNode(_options, this, null, null, key, value)));
					children.Add(new IndexValue(Int32.MaxValue, null));

					return new InsertInfo();
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (key <= children[i].index)
					{
						// Update then
						if (key == children[i].index)
						{
							children[i].node.Insert(key, value);
							updated = true;
						}
						else
						{
							Node prev = null;
							if (i != 0)
							{
								prev = children[i - 1].node;
							}
							else if (children[i].node != null)
							{
								prev = children[i].node.prev;
							}

							var newNode = new DataNode(
								_options,
								this,
								children[i].node,
								prev,
								key,
								value
							);
							children.Insert(i, new IndexValue(key, newNode));

							// update neighbors
							// it must be the case that the neighbor of new node exists
							if (newNode.next != null)
							{
								newNode.next.prev = newNode;
							}

							if (newNode.prev != null)
							{
								newNode.prev.next = newNode;
							}
						}

						break;
					}
				}

				if (children.Count > _options.Branching)
				{
					// Split
					var half = children.Count / 2 + children.Count % 2;

					var newNodeChildren = this.children.Skip(half).ToList();
					var newNode = new LeafNode(_options, this.parent, this.next, this, newNodeChildren);

					if (this.next != null)
					{
						this.next.prev = newNode;
					}
					this.next = newNode;

					children = children.Take(half).ToList();

					return new InsertInfo
					{
						extraNode = newNode,
						updated = updated
					};
				}

				return new InsertInfo {
					updated = updated
				};
			}

			public override string TypeString()
			{
				return "L";
			}

			public override bool Validate(bool isRoot)
			{
				bool atLeastOneChild = children.Count > 0;
				bool noUnderflow = isRoot || children.Count >= (_options.Branching / 2) + (_options.Branching % 2) - 1;
				bool overflow = children.Count > _options.Branching;
				bool childrenOrdered =
					children
						.Zip(
							children.Skip(1),
							(a, b) => new { a, b }
						)
						.All(pair => pair.a.index < pair.b.index);

				return
					atLeastOneChild &&
					noUnderflow &&
					!overflow &&
					childrenOrdered &&
					children.Where(ch => ch.node != null).All(ch => ch.node.Validate());
			}

			protected override bool IsUnderflow()
			{
				return
					(children.Count < (_options.Branching / 2) + (_options.Branching % 2) - 1) ||
					(children.Count == 1 && children[0].node == null); // or the only node is shadow infinity
			}
		}

	}
}
