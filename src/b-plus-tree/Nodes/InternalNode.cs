using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class InternalNode : Node
		{
			public InternalNode(Options options) : base(options) { }

			public InternalNode(Options options, List<IndexValue> children) : base(options)
			{
				this.children = children;
			}

			public override Node Insert(int key, T value)
			{
				Node extraNode = null;
				Node prevNode = null;

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (key >= children[i].index && key <= children[i + 1].index)
					{
						extraNode = children[i + 1].node.Insert(key, value);
						prevNode = children[i + 1].node;

						break;
					}
				}

				if (extraNode == null)
				{
					return null;
				}

				var newKey = prevNode.LargestIndex();

				for (int i = 0; i < children.Count; i++)
				{
					if (children[i].node == prevNode)
					{
						children[i] = new IndexValue(children[i].index, extraNode);

						children.Insert(i, new IndexValue(newKey, prevNode));
						break;
					}
				}

				if (children.Count == _options.Branching + 2)
				{
					// Split
					var half = children.Count / 2 + children.Count % 2;

					var newNodeChildren = this.children.Skip(half).ToList();
					newNodeChildren.Insert(0, new IndexValue(Int32.MinValue, null));
					var newNode = new InternalNode(_options, newNodeChildren);

					children = children.Take(half).ToList();
					children.Add(new IndexValue(Int32.MaxValue, null));

					return newNode;
				}

				return null;
			}

			public override string TypeString()
			{
				return "I";
			}
		}
	}
}
