using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public class State
	{
		internal IPRG _G;
		private ulong min;
		private ulong max;

		private Dictionary<int, Tuple<ulong, ulong>> _minMax;

		private Node _root = null;

		public State(IPRG prg, ulong min, ulong max)
		{
			_G = prg;
			this.min = min;
			this.max = max;

			_minMax = new Dictionary<int, Tuple<ulong, ulong>>();
		}

		internal ulong Insert(int plaintext)
		{
			ulong result;

			if (_root == null)
			{
				var newCipher = (ulong)Math.Ceiling(0.5 * min) + (ulong)Math.Ceiling(0.5 * max);
				_root = new Node(_G, plaintext, newCipher);

				result = newCipher;
			}
			else
			{
				result = _root.Insert(plaintext, min, max) ?? Rebalance(plaintext);
			}

			if (!_minMax.ContainsKey(plaintext))
			{
				_minMax.Add(plaintext, new Tuple<ulong, ulong>(result, result));
			}
			else
			{
				var current = _minMax[plaintext];
				_minMax[plaintext] = new Tuple<ulong, ulong>(Math.Min(result, current.Item1), Math.Max(result, current.Item2));
			}

			return result;
		}

		internal int Get(ulong input)
		{
			if (_root == null)
			{
				throw new InvalidOperationException($"Ciphertext {input} was never produced.");
			}
			else
			{
				return _root.Get(input);
			}
		}

		internal ulong GetMinMaxCipher(int plaintext, bool min)
		{
			if (_root == null || !_minMax.ContainsKey(plaintext))
			{
				throw new InvalidOperationException($"Plaintext {plaintext} was never encrypted.");
			}
			else
			{
				return min ? _minMax[plaintext].Item1 : _minMax[plaintext].Item2;
			}
		}

		private ulong Rebalance(int input)
		{
			var plaintexts = new List<int>();
			_root.GetAll(plaintexts);

			plaintexts.Add(input);
			plaintexts.OrderBy(p => p);

			_root = null;

			var insertQueue = new Queue<Tuple<int, int>>();

			Action<int, int> insert =
				(from, to) =>
				{
					var index = (int)Math.Ceiling((to + from) * 0.5);
					Insert(plaintexts[index]);

					if (from != index)
					{
						insertQueue.Enqueue(new Tuple<int, int>(from, index - 1));
					}

					if (to != index)
					{
						insertQueue.Enqueue(new Tuple<int, int>(index + 1, to));
					}
				};

			insert(0, plaintexts.Count - 1);

			while (insertQueue.Count > 0)
			{
				var tuple = insertQueue.Dequeue();
				insert(tuple.Item1, tuple.Item2);
			}

			return _root.Insert(input, min, max, get: true).Value;
		}

		private class Node
		{
			private IPRG _G;

			public Node(IPRG prg, int plaintext, ulong ciphertext)
			{
				_G = prg;
				this.plaintext = plaintext;
				this.ciphertext = ciphertext;
			}

			private Node left = null;
			private Node right = null;

			private int plaintext;
			private ulong ciphertext;

			public ulong? Insert(int input, ulong min, ulong max, bool get = false)
			{
				bool? coin = null;

				if (input == plaintext)
				{
					if (get)
					{
						return ciphertext;
					}
					coin = _G.Next() % 2 == 1;
				}

				if (
					input > plaintext ||
					(coin.HasValue && coin.Value == true)
				)
				{
					if (right != null)
					{
						return right.Insert(input, ciphertext, max);
					}
					else
					{
						if (max - ciphertext < 2)
						{
							return null;
						}
					}

					var newCipher = (ulong)Math.Ceiling(0.5 * ciphertext) + (ulong)Math.Ceiling(0.5 * max);
					right = new Node(_G, input, newCipher);

					return newCipher;
				}

				if (
					input < plaintext ||
					(coin.HasValue && coin.Value == false)
				)
				{
					if (left != null)
					{
						return left.Insert(input, min, ciphertext);
					}
					else
					{
						if (ciphertext - min < 2)
						{
							return null;
						}
					}

					var newCipher = (ulong)Math.Ceiling(0.5 * min) + (ulong)Math.Ceiling(0.5 * ciphertext);
					left = new Node(_G, input, newCipher);

					return newCipher;
				}

				throw new InvalidOperationException("Should never be here.");
			}

			public int Get(ulong input)
			{
				if (input > ciphertext)
				{
					if (right == null)
					{
						throw new InvalidOperationException($"Ciphertext {input} was never produced.");
					}
					return right.Get(input);
				}
				else if (input < ciphertext)
				{
					if (left == null)
					{
						throw new InvalidOperationException($"Ciphertext {input} was never produced.");
					}
					return left.Get(input);
				}

				return plaintext;
			}

			public void GetAll(List<int> result)
			{
				result = result ?? new List<int>();

				if (left != null)
				{
					left.GetAll(result);
				}

				result.Add(plaintext);

				if (right != null)
				{
					right.GetAll(result);
				}
			}
		}
	}
}
