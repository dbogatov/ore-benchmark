using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T, C>
	{
		private class DataNode : Node
		{
			public C key;
			public T value;

			public DataNode(Options<C> options, Node parent, Node next, Node prev, C key, T value) : base(options, parent, next, prev)
			{
				this.key = key;
				this.value = value;
			}

			public override bool TryGet(C key, out T value, bool checkValue = true)
			{
				_options.OnVisit(this.GetHashCode());

				bool found = true;

				if (checkValue)
				{
					found = _options.Comparator.IsEqual(this.key, key);
				}

				if (found)
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

			public override InsertInfo Insert(C key, T value)
			{
				_options.OnVisit(this.GetHashCode());

				this.value = value;
				return new InsertInfo
				{
					extraNode = this
				};
			}

			public override DeleteInfo Delete(C key)
			{
				_options.OnVisit(this.GetHashCode());

				if (_options.Comparator.IsEqual(this.key, key))
				{
					ConnectNeighbors();

					return new DeleteInfo
					{
						orphan = this
					};
				}
				else
				{
					return new DeleteInfo
					{
						notFound = true
					};
				}
			}

			public override C LargestIndex()
			{
				_options.OnVisit(this.GetHashCode());

				return this.key;
			}

			protected override void Initialize() { }

			public override string ToString(int level, bool last, List<bool> nests, C index)
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
				// Should never be called
				return false;
			}

			public override bool CheckIndexes()
			{
				return true;
			}

			public override bool CheckNeighborLinks(bool leftMost = false, bool isRoot = false)
			{
				var result =
					(this.parent != null) &&
					(this.next == null || this.next.CheckNeighborLinks());

				return result;
			}
		}
	}
}
