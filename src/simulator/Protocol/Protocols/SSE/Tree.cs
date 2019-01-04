using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Protocol.SSE
{
	public class Tree<K> where K : ORESchemes.CJJKRS.IWord
	{
		private class Options
		{
			public Func<int, int, K> indexToKeyword;
		}

		private class ValueNext
		{
			public int Value { get; set; }
			public ValueNext Next { get; set; }
		}

		private class Node
		{
			private Options _options;

			private Node _left;
			private Node _right;
			protected Node _parent;
			protected K _keyword;
			private int _index;
			private int _level;

			public Node(int index, int level, Node parent, Options options)
			{
				_options = options;
				_parent = parent;
				_index = index;
				_level = level;
				_keyword = options.indexToKeyword(index, level);
			}

			public void Insert(ValueNext value, int min, int max)
			{
				var middle = (max + min) / 2;
				if (max - min <= 2)
				{
					if (value.Value <= middle)
					{
						_left = new Leaf(value, _level + 1, this, _options);
					}
					else
					{
						_right = new Leaf(value, _level + 1, this, _options);
					}
				}
				else
				{
					if (value.Value <= middle)
					{
						if (_left == null)
						{
							_left = new Node((min + middle) / 2, _level + 1, this, _options);
						}
						_left.Insert(value, min, middle);
					}
					else
					{
						if (_right == null)
						{
							_right = new Node((middle + max) / 2, _level + 1, this, _options);
						}
						_right.Insert(value, middle, max);
					}
				}
			}

			public virtual Node Find(int index)
			{
				if (_left != null && _right != null)
				{
					return (index <= _index ? _left : _right).Find(index);
				}

				if (_left != null)
				{
					return _left.Find(index);
				}

				if (_right != null)
				{
					return _right.Find(index);
				}

				throw new InvalidOperationException("Should not be here");
			}

			private Node Rightmost() => Find(int.MaxValue);

			private bool AmILeft(Node child) => _left == child;

			public (K, int) RangeKeyword()
			{
				if (_parent == null || !_parent.AmILeft(this))
				{
					return (_keyword, ((Leaf)Rightmost()).Value);
				}
				else
				{
					return _parent.RangeKeyword();
				}
			}

			public List<K> YieldKeywords(List<K> accumulator = null)
			{
				if (accumulator == null)
				{
					accumulator = new List<K>();
				}

				if (_parent != null)
				{
					_parent.YieldKeywords(accumulator);
				}

				accumulator.Add(_keyword);

				return accumulator;
			}
		}

		private class Leaf : Node
		{
			private ValueNext _value;

			public Leaf(ValueNext value, int level, Node parent, Options options) : base(value.Value, level, parent, options)
			{
				_value = value;
			}

			public override Node Find(int index) => this;

			public int Value { get => _value.Value; }
		}

		private Node _root;
		private ValueNext _first;

		public Tree(int min, int max, List<int> indices, Func<int, int, K> keywordGenerator)
		{
			var middle = (max + min) / 2;
			_root = new Node(middle, 0, null, new Options { indexToKeyword = keywordGenerator });
			indices = indices.OrderBy(v => v).ToList();

			var tuples = indices.OrderBy(v => v).Select(i => new ValueNext { Value = i }).ToArray();

			for (int i = 0; i < tuples.Count() - 1; i++)
			{
				tuples[i].Next = tuples[i + 1];
			}

			foreach (var tuple in tuples)
			{
				_root.Insert(tuple, min, max);
			}

			_first = tuples.FirstOrDefault();
		}

		internal int? FirstInRange(int min)
		{
			var current = _first;
			while (current != null)
			{
				if (current.Value >= min)
				{
					return current.Value;
				}
				current = current.Next;
			}

			return null;
		}

		public List<K> BRC(int min, int max)
		{
			var keywords = new List<K>();

			while (min <= max)
			{
				var minIndex = FirstInRange(min);

				if (minIndex.HasValue)
				{
					var leftmost = _root.Find(minIndex.Value);
					(var keyword, var lastCovered) = leftmost.RangeKeyword();
					keywords.Add(keyword);
					min = lastCovered + 1;
				}
				else
				{
					break;
				}
			}

			return keywords;
		}

		public List<K> KeywordsForValue(int value)
		{
			var leaf = _root.Find(value);
			return leaf.YieldKeywords();
		}
	}
}
