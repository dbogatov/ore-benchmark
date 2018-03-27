using System;
using System.Collections.Generic;
using OPESchemes;

namespace DataStructures.BPlusTree
{
	public partial class Tree<T>
	{
		private readonly Options _options;
		private readonly IOPEScheme _scheme;

		private int _size = 0;

		private Node _root;

		public Tree(Options options)
		{
			_options = options;
			_scheme = OPESchemesFactory.GetScheme(options.Scheme);
			_root = new LeafNode(options, null, null, null);
		}

		public bool TryGet(int key, out T value)
		{
			value = default(T);

			if (_size == 0)
			{
				return false;
			}

			return _root.TryGet(key, out value);
		}

		public bool TryRange(int start, int end, out List<T> values)
		{
			values = new List<T>();

			if (start >= end)
			{
				throw new ArgumentException("Improper range");
			}

			if (_size == 0)
			{
				return false;
			}

			return _root.TryRange(start, end, values);
		}

		public void Insert(int key, T value)
		{
			var extraNode = _root.Insert(key, value);

			if (extraNode != null)
			{
				// Root split
				var prevRoot = _root;

				_root = new InternalNode(
					_options,
					null,
					null,
					null,
					new List<IndexValue> {
						new IndexValue(prevRoot.LargestIndex(), prevRoot),
						new IndexValue(Int32.MaxValue, extraNode)
					}
				);
			}

			_size++;
		}

		public bool Delete(int key)
		{
			var result = _root.Delete(key, true);

			if (result.notFound)
			{
				return false;
			}

			if (result.onlyChild != null)
			{
				_root = result.onlyChild;
			}

			_size--;

			// Check if this is necessary
			if (_size == 0)
			{
				_root = new LeafNode(_options, null, null, null);
			}

			return true;
		}

		public int Size()
		{
			return _size;
		}

		public override string ToString()
		{
			return "Tree: \n" + _root.ToString(1, true, new List<bool> { false }, Int32.MinValue);
		}

		public void Validate()
		{
			if (!_root.isBalanced())
			{
				throw new InvalidOperationException("Tree is not balanced");
			}

			_root.Validate(true);

			_root.CheckNeighborLinks(true);
		}
	}
}
