using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{

		private class DataNode : Node
		{
			public T value;

			public DataNode(Options options, T value) : base(options)
			{
				this.value = value;
			}

			public override bool TryGet(int key, out T value)
			{
				value = this.value;
				return true;
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
		}
	}
}
