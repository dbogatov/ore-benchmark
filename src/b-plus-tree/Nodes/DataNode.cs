using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class DataNode : Node
		{
			public int key;
			public T value;

			public DataNode(Options options, Node parent, Node next, Node prev, int key, T value) : base(options, parent, next, prev)
			{
				this.key = key;
				this.value = value;
			}

			public override bool TryGet(int key, out T value)
			{
				if (this.key == key)
				{
					value = this.value;
					return true;
				}
				else
				{
					value = default(T);
					return false;
				}
			}

			public override Node Insert(int key, T value)
			{
				this.value = value;
				return this;
			}

			public override DeleteInfo Delete(int key)
			{
				return
					this.key == key ?
					new DeleteInfo
					{
						orphan = this
					} :
					new DeleteInfo
					{
						notFound = true
					};
			}


			protected override void Initialize() { }

			public override string ToString(int level, bool last, List<bool> nests, int index)
			{
				var result = "    ";

				for (int i = 0; i < level - 1; i++)
				{
					result += nests[i] ? "│   " : "    ";
				}

				return result + $"{(last ? "└" : "├")}[{index.ToString().PadRight(3)}]──\"{value}\"\n";
			}

			public override string TypeString()
			{
				return "D";
			}

			protected override int Height()
			{
				return 1;
			}

			public override bool isBalanced()
			{
				return true;
			}

			protected override bool IsUnderflow()
			{
				return false;
			}

			public override bool CheckNeighborLinks(bool leftMost = false)
			{
				return true;
			}
		}
	}
}
