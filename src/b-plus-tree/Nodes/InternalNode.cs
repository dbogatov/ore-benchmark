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
					var newNode = new InternalNode(_options, newNodeChildren);

					children = children.Take(half).ToList();

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
