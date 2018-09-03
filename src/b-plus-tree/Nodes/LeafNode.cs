using System.Collections.Generic;
using System.Linq;

namespace BPlusTree
{
	internal partial class Tree<T, C>
	{
		private class LeafNode : Node
		{
			public LeafNode(Options<C> options, Node parent, Node next, Node prev) : base(options, parent, next, prev) { }

			public LeafNode(Options<C> options, Node parent, Node next, Node prev, List<IndexValue> children) : base(options, parent, next, prev)
			{
				this.children = children;
				this.children.Where(ch => ch.node != null).ToList().ForEach(ch => ch.node.parent = this);
			}

			public override bool TryRange(C start, C end, List<Data> values)
			{
				_options.OnVisit(this.GetHashCode());

				var found = false;

				for (int i = 0; i < children.Count; i++)
				{
					if (
						_options.Comparator.IsLessOrEqual(start, children[i].index) &&
						_options.Comparator.IsGreaterOrEqual(end, children[i].index) &&
						children[i].node != null)
					{
						found = true;

						children[i].node.TryGet(children[i].index, values, checkValue: false);
					}
				}

				if (found)
				{
					LeafNode nextLeaf = (LeafNode)next;
					while (nextLeaf != null)
					{
						nextLeaf = nextLeaf.ReturnRange(_options.MinCipher, end, values);
					}
				}

				return found;
			}

			/// <summary>
			/// A non-recursive version of leaf's TryRange
			/// A recursive one would trigger StackOverflow exception
			/// </summary>
			/// <param name="start">Start of the search range</param>
			/// <param name="end">End of the search range</param>
			/// <param name="values">List to append results to</param>
			/// <returns>The next leaf if at least one element was found, null otherwise</returns>
			protected LeafNode ReturnRange(C start, C end, List<Data> values)
			{
				_options.OnVisit(this.GetHashCode());

				var found = false;

				for (int i = 0; i < children.Count; i++)
				{
					if (
						_options.Comparator.IsLessOrEqual(start, children[i].index) &&
						_options.Comparator.IsGreaterOrEqual(end, children[i].index) &&
						children[i].node != null)
					{
						found = true;

						children[i].node.TryGet(children[i].index, values, checkValue: false);
					}
					else
					{
						break;
					}
				}

				return found ? (LeafNode)next : null;
			}

			public override InsertInfo Insert(C key, T value)
			{
				_options.OnVisit(this.GetHashCode());

				var updated = false;

				if (children.Count == 0)
				{
					children.Add(new IndexValue(key, new DataNode(_options, this, null, null, key, value)));
					children.Add(new IndexValue(_options.MaxCipher, null));

					return new InsertInfo();
				}

				for (int i = 0; i < children.Count; i++)
				{
					if (_options.Comparator.IsLessOrEqual(key, children[i].index))
					{
						// Update then
						if (_options.Comparator.IsEqual(key, children[i].index))
						{
							var info = children[i].node.Insert(key, value);
							updated = info.updated;
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

				return new InsertInfo
				{
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
						.All(pair => _options.Comparator.IsLess(pair.a.index, pair.b.index));

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
