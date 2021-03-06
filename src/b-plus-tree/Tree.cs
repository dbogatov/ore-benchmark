﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Crypto.Shared;

[assembly: InternalsVisibleTo("simulator")]
[assembly: InternalsVisibleTo("test")]

namespace BPlusTree
{
	public interface ITree<T, C>
	{
		/// <summary>
		/// Updates a single element of the tree
		/// </summary>
		/// <param name="key">Index of the updated element</param>
		/// <param name="value">New value</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <returns>True if element was found, false otherwise</returns>
		/// <exception cref="System.InvalidOperationException">Thrown if more than one element were retrivied for the index with predicate</exception>  
		bool UpdateSingle(C key, T value, Func<T, bool> predicate = null);

		/// <summary>
		/// Updates all matched elements of the tree
		/// </summary>
		/// <param name="key">Index of the updated element</param>
		/// <param name="value">New value</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <returns>True if element was found, false otherwise</returns>
		bool Update(C key, T value, Func<T, bool> predicate = null);

		/// <summary>
		/// Returns a value of single element of the tree
		/// </summary>
		/// <param name="key">Index of the element to find</param>
		/// <param name="value">Variable to place value to</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <returns>True if element was found, false otherwise</returns>
		bool TryGetSingle(C key, out T value, Func<T, bool> predicate = null);

		/// <summary>
		/// Returns all values of matched elements of the tree
		/// </summary>
		/// <param name="key">Index of the element to find</param>
		/// <param name="values">List to place values to (should not be null)</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <returns>True if element was found, false otherwise</returns>
		bool TryGet(C key, List<T> values, Func<T, bool> predicate = null);

		/// <summary>
		/// Returns the value for the key range (both ends inclusive)
		/// </summary>
		/// <param name="start">Key for start of the range</param>
		/// <param name="end">Key for end of the range</param>
		/// <param name="values">The list to put found values to (should not be null)</param>
		/// <param name="checkRanges">If unset that ranges check would be skipped</param>
		/// <returns>True if at least element found, false otherwise</returns>
		bool TryRange(C start, C end, List<T> values, bool checkRanges = true);

		/// <summary>
		/// Inserts or updates the value for the key
		/// </summary>
		/// <param name="key">Key for value</param>
		/// <param name="value">Value to insert or update with</param>
		/// <returns>True if the value with this key did not exist, false otherwise</returns>
		bool Insert(C key, T value);

		/// <summary>
		/// Remove an element with a given key from the tree
		/// </summary>
		/// <param name="key">The key to remove</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <returns>True if element was found, false otherwise</returns>
		bool Delete(C key, Func<T, bool> predicate = null);

		/// <summary>
		/// Returns the size of tree in number of elements
		/// </summary>
		/// <returns>The size of tree in number of elements</returns>
		int Size();
		
		/// <summary>
		/// Returns the size of tree in number of nodes
		/// </summary>
		/// <param name="includeDataNodes">Whether to include data nodes in the calculation</param>
		/// <returns>The size of tree in number of nodes</returns>
		int Nodes(bool includeDataNodes = true);

		/// <summary>
		/// Runs checks to verify the integrity of the tree
		/// Needed for proper testing
		/// </summary>
		/// <returns>True if tree satisfies all constraints, false otherwise</returns>
		bool Validate();
	}

	/// <typeparam name="T">Data type</typeparam>
	/// <typeparam name="C">Ciphertext type</typeparam>
	internal partial class Tree<T, C> : ITree<T, C>
	{
		private readonly Options<C> _options;

		private int _size = 0;

		private Node _root;

		public Tree(Options<C> options)
		{
			_options = options;
			_root = new LeafNode(options, null, null, null);
		}

		public bool UpdateSingle(C key, T value, Func<T, bool> predicate = null)
			=> RetriveRoutine(
				key: key,
				values: null,
				value: value,
				predicate: predicate,
				get: false,
				single: true
			);

		public bool Update(C key, T value, Func<T, bool> predicate = null)
			=> RetriveRoutine(
				key: key,
				values: null,
				value: value,
				predicate: predicate,
				get: false,
				single: false
			);

		public bool TryGetSingle(C key, out T value, Func<T, bool> predicate = null)
		{
			value = default(T);

			var values = new List<T>();

			var found = RetriveRoutine(
			   key: key,
			   values: values,
			   value: default(T),
			   predicate: predicate,
			   get: true,
			   single: true
		   );

			if (found)
			{
				value = values.Single();
			}

			return found;
		}

		public bool TryGet(C key, List<T> values, Func<T, bool> predicate = null)
			=> RetriveRoutine(
				key: key,
				values: values,
				value: default(T),
				predicate: predicate,
				get: true,
				single: false
			);

		/// <summary>
		/// Generic search routine that finds elements of tree matched by search criteria
		/// and performs some operations on them (get or update)
		/// </summary>
		/// <param name="key">Index of the element to find</param>
		/// <param name="values">List to place values to</param>
		/// <param name="value">New value for update operation</param>
		/// <param name="predicate">Predicate to use for values of the requested index</param>
		/// <param name="get">If set, then search will performed and result will be given to supplied list</param>
		/// <param name="single">If set, exception will be thrown if search resulted in multiple elements</param>
		/// <returns>True if element was found, false otherwise</returns>
		///<exception cref="InvalidOperationException">Thrown if more than one element were retrivied for the index with predicate</exception>  
		private bool RetriveRoutine(C key, List<T> values, T value, Func<T, bool> predicate, bool get, bool single)
		{
			values = values ?? new List<T>();
			var returned = new List<Data>();

			if (_size == 0)
			{
				return false;
			}

			var found = _root.TryGet(key, returned, predicate);
			if (found)
			{
				if (single && returned.Count > 1)
				{
					throw new InvalidOperationException("...Single operation resulted in multiple records.");
				}
				if (get)
				{
					values.AddRange(returned.Select(d => d.data).ToList());
				}
				else
				{
					returned.ForEach(r => r.data = value);
				}
			}

			return found;
		}

		public bool TryRange(C start, C end, List<T> values, bool checkRanges = true)
		{
			values = values ?? new List<T>();
			var returned = new List<Data>();

			if (checkRanges && _options.Comparator.IsGreaterOrEqual(start, end))
			{
				throw new ArgumentException("Improper range");
			}

			if (_size == 0)
			{
				return false;
			}

			var found = _root.TryRange(start, end, returned);
			if (found)
			{
				values.AddRange(returned.Select(r => r.data).ToList());
			}

			return found;
		}

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
						new IndexValue(_options.MaxCipher, extraNode)
					}
				);
			}

			_size++;

			return !result.updated;
		}

		public bool Delete(C key, Func<T, bool> predicate = null)
		{
			var result = _root.Delete(key, predicate);

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

		public int Size() => _size;
		
		public int Nodes(bool includeDataNodes = true) => _size > 0 ? _root.Nodes(includeDataNodes) : 0;

		public override string ToString()
			=> "Tree: \n" + _root.ToString(1, true, new List<bool> { false }, _options.MinCipher);

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

	/// <summary>
	/// A wrapper around Tree class.
	/// This class is designed to be used by package users.
	/// </summary>
	/// <typeparam name="T">Type of data in a tree</typeparam>
	public class BPlusTree<T> : ITree<T, long>
	{
		private readonly Tree<T, OPECipher> _tree;

		public BPlusTree(int branches = 60)
		{
			var options = new Options<OPECipher>(
				new NoEncryptionScheme(),
				branches
			);
			options.MinCipher = long.MinValue;
			options.MaxCipher = long.MaxValue;

			_tree = new Tree<T, OPECipher>(options);
		}

		public bool UpdateSingle(long key, T value, Func<T, bool> predicate = null) =>
			_tree.UpdateSingle(new OPECipher(key), value, predicate);

		public bool Update(long key, T value, Func<T, bool> predicate = null) =>
			_tree.Update(new OPECipher(key), value, predicate);

		public bool TryGetSingle(long key, out T value, Func<T, bool> predicate = null) =>
			_tree.TryGetSingle(new OPECipher(key), out value, predicate);

		public bool TryGet(long key, List<T> values, Func<T, bool> predicate = null) =>
			_tree.TryGet(new OPECipher(key), values, predicate);

		public bool TryRange(long start, long end, List<T> values, bool checkRanges = true) =>
			_tree.TryRange(new OPECipher(start), new OPECipher(end), values, checkRanges);

		public bool Insert(long key, T value) =>
			_tree.Insert(new OPECipher(key), value);

		public bool Delete(long key, Func<T, bool> predicate = null) =>
			_tree.Delete(new OPECipher(key), predicate);

		public int Size() => _tree.Size();
		
		public int Nodes(bool includeDataNodes) => _tree.Nodes(includeDataNodes = true);

		public override string ToString() => _tree.ToString();

		public bool Validate() => _tree.Validate();
	}
}
