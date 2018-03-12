using System;
using System.Collections.Generic;
using OPESchemes;

namespace DataStructures.BPlusTree
{
	public class Options
	{
		public int Branching { get; private set; }
		public double Occupancy { get; private set; }
		public OPESchemes.OPESchemes Scheme { get; private set; }

		public Options(
			OPESchemes.OPESchemes scheme = OPESchemes.OPESchemes.NoEncryption,
			int branching = 60,
			double occupancy = 0.7
		)
		{
			if (
				branching < 3 || branching > 65536 ||
				occupancy < 0.5 || occupancy > 0.9
			)
			{
				throw new ArgumentException("Bad B+ tree options");
			}

			Branching = branching;
			Occupancy = occupancy;
			Scheme = scheme;
		}
	}

	public class Tree<T>
	{
		private readonly Options _options;
		private readonly IOPEScheme _scheme;

		public int Size { get; private set; }

		private InternalNode _root;

		public Tree(Options options)
		{
			_options = options;
			_scheme = OPESchemesFactory.GetScheme(options.Scheme);

			Size = 0;
			_root = new InternalNode(options);
		}

		public bool TryGet(int key, out T value)
		{
			value = default(T);

			if (Size == 0)
			{
				return false;
			}

			return _root.TryGet(key, out value);
		}

		public void Add(T element)
		{


			Size++;
		}

		private abstract class Node
		{
			protected readonly Options _options;
			public List<Tuple<int, Node>> children;

			public Node(Options options)
			{
				_options = options;
				children = new List<Tuple<int, Node>>(options.Branching);
			}

			public virtual bool TryGet(int key, out T value)
			{
				value = default(T);

				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (key >= children[i].Item1 && key <= children[i + 1].Item1)
					{
						if (children[i].Item2 == null)
						{
							return false;
						}

						return children[i].Item2.TryGet(key, out value);
					}
				}

				return false;
			}

			public virtual bool TryRange(int start, int end, List<T> values = null)
			{
				if (start >= end)
				{
					throw new ArgumentException("Improper range");
				}

				if (values == null)
				{
					values = new List<T>();
				}

				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (start >= children[i].Item1 && start <= children[i + 1].Item1)
					{
						if (children[i].Item2 == null)
						{
							return false;
						}

						return children[i].Item2.TryRange(start, end, values);
					}
				}

				return false;
			}
		}

		private class InternalNode : Node
		{
			public InternalNode(Options options) : base(options) { }
		}

		private class LeafNode : Node
		{
			public LeafNode prev = null;
			public LeafNode next = null;

			public LeafNode(Options options) : base(options) { }

			public override bool TryRange(int start, int end, List<T> values = null)
			{
				if (children.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < children.Count - 1; i++)
				{
					if (start >= children[i].Item1 && start <= children[i + 1].Item1)
					{
						if (children[i].Item2 == null)
						{
							return false;
						}

						for (int j = i; j < children.Count - 1 - i; j++)
						{
							T value;
							children[i].Item2.TryGet(children[i].Item1, out value);
							values.Add(value);
						}

						next.TryRange(Int32.MinValue, end, values);

						return true;
					}
				}

				return false;
			}
		}

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
		}
	}
}
