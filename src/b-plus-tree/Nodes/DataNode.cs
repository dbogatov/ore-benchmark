using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T, C>
	{
		private class Data
		{
			public T data;

			public Data(T data) => this.data = data;

			public override string ToString() => data.ToString();
		}

		private class DataNode : Node
		{
			public C key;
			public List<Data> values;

			public DataNode(Options<C> options, Node parent, Node next, Node prev, C key, T value) : base(options, parent, next, prev)
			{
				this.key = key;
				this.values = new List<Data>() { new Data(value) };
			}

			public override bool TryGet(C key, List<Data> values, Func<T, bool> predicate = null, bool checkValue = true)
			{
				_options.OnVisit(this.GetHashCode());

				bool found = true;

				if (checkValue)
				{
					found = _options.Comparator.IsEqual(this.key, key);
				}

				if (found)
				{
					if (predicate != null)
					{
						var result = this.values.Where(v => predicate(v.data));
						values.AddRange(result);

						found = result.Count() > 0;
					}
					else
					{
						values.AddRange(this.values);
					}
				}

				return found;
			}

			public override InsertInfo Insert(C key, T value)
			{
				_options.OnVisit(this.GetHashCode());

				bool updated = this.values.Count > 0;
				this.values.Add(new Data(value));

				return new InsertInfo
				{
					extraNode = this,
					updated = updated
				};
			}

			public override DeleteInfo Delete(C key, Func<T, bool> predicate = null)
			{
				_options.OnVisit(this.GetHashCode());

				if (_options.Comparator.IsEqual(this.key, key))
				{
					if (predicate != null)
					{
						values.RemoveAll(v => predicate(v.data));
					}
					else
					{
						values.Clear();
					}

					if (values.Count == 0)
					{
						ConnectNeighbors();

						return new DeleteInfo
						{
							orphan = this
						};
					}
					else
					{
						return new DeleteInfo();
					}
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

				return result + $"{(last ? "└" : "├")}[{index.ToString().PadRight(3)}]──{string.Join(",", values.Select(v => $"\"{v}\""))}\n";
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
