using System;
using System.Collections.Generic;
using ORESchemes.Shared;

namespace DataStructures.BPlusTree
{
	/// <summary>
	/// T is a data in the data node type
	/// C is a ciphertext (index) type
	/// </summary>
	public partial class Tree<T, C>
	{
		private readonly Options<C> _options;

		private int _size = 0;

		private Node _root;

		public Tree(Options<C> options)
		{
			_options = options;
			_root = new LeafNode(options, null, null, null);
		}

		/// <summary>
		/// Returns the value for the key
		/// </summary>
		/// <param name="key">Search key</param>
		/// <param name="value">Variable to place value to</param>
		/// <returns>True if element is found, false otherwise</returns>
		public bool TryGet(C key, out T value)
		{
			value = default(T);

			if (_size == 0)
			{
				return false;
			}

			return _root.TryGet(key, out value);
		}

		/// <summary>
		/// Returns the value for the key range (both ends inclusive)
		/// </summary>
		/// <param name="start">Key for start of the range</param>
		/// <param name="end">Key for end of the range</param>
		/// <param name="values">The list to put found value to</param>
		/// <returns>True if at least element found, false otherwise</returns>
		public bool TryRange(C start, C end, out List<T> values)
		{
			values = new List<T>();

			if (_options.Scheme.IsGreaterOrEqual(start, end))
			{
				throw new ArgumentException("Improper range");
			}

			if (_size == 0)
			{
				return false;
			}

			return _root.TryRange(start, end, values);
		}

		/// <summary>
		/// Inserts or updates the value for the key
		/// </summary>
		/// <param name="key">Key for value</param>
		/// <param name="value">Value to insert or update with</param>
		/// <returns>True if the value was inserted, false if the value was updated</returns>
		public bool Insert(C key, T value)
		{
			var result = _root.Insert(key, value);
			var extraNode = result.extraNode;

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
						new IndexValue(_options.Scheme.MaxCiphertextValue(), extraNode)
					}
				);
			}

			_size++;

			return !result.updated;
		}

		/// <summary>
		/// Remove an element with a given key from the tree
		/// </summary>
		/// <param name="key">The key to remove</param>
		/// <returns>True if element was found, false otherwise</returns>
		public bool Delete(C key)
		{
			var result = _root.Delete(key);

			if (result.notFound)
			{
				return false;
			}

			if (result.onlyChild != null)
			{
				_root = result.onlyChild;
				_root.parent = null; // kill old skin
			}

			_size--;

			if (_size == 0)
			{
				_root = new LeafNode(_options, null, null, null);
			}

			return true;
		}

		/// <summary>
		/// Returns the size of tree in number of elements
		/// </summary>
		/// <returns>The size of tree in number of elements</returns>
		public int Size()
		{
			return _size;
		}

		public override string ToString()
		{
			return "Tree: \n" + _root.ToString(1, true, new List<bool> { false }, _options.Scheme.MinCiphertextValue());
		}

		/// <summary>
		/// Runs checks to verify the integrity of the tree
		/// Needed for proper testing
		/// </summary>
		/// <returns>True if tree satisfies all constraints, false otherwise</returns>
		public bool Validate()
		{
			if (_size == 0)
			{
				return true;
			}

			var balanced = _root.isBalanced();

			var indexes = _root.CheckIndexes();

			var links = _root.CheckNeighborLinks(true, true);

			var invariants = _root.Validate(true);

			return
				balanced &&
				indexes &&
				links &&
				invariants;
		}
	}
}
