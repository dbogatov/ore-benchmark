using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public delegate void MutationEventHandler();

	public class State
	{
		public event MutationEventHandler MutationOcurred;

		private IPRG _G;
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

		/// <summary>
		/// Adds a plaintext to the structure
		/// </summary>
		/// <param name="plaintext">Plaintex to insert</param>
		/// <returns>A ciphertext generated during insertion</returns>
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

		/// <summary>
		/// Retrives plaintext for given ciphertext from data structure
		/// </summary>
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

		/// <summary>
		/// Returns the minimal or maximal ciphertex for the given plaintext
		/// </summary>
		/// <param name="plaintext">Plaintext for which to return ciphertexts</param>
		/// <param name="min">True, if mimimal is requested</param>
		/// <returns>Corresponding ciphertext</returns>
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

		/// <summary>
		/// Recreates the whole tree making it balanced.
		/// Regenerates ciphertexts.
		/// </summary>
		/// <param name="input">One more plaintext to insert to the rebalanced tree</param>
		/// <returns>
		/// The ciphertext from the new rebalanced tree corresponding to the supplied plaintext
		/// </returns>
		private ulong Rebalance(int input)
		{
			var handler = MutationOcurred;
			if (handler != null)
			{
				handler();
			}

			_minMax.Clear();

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

			/// <summary>
			/// Adds input as a plaintex to data structure
			/// </summary>
			/// <param name="input">Plaintext to add</param>
			/// <param name="min">Minimal possible value of ciphertext</param>
			/// <param name="max">Maximal possible value of ciphertext</param>
			/// <param name="get">If true, when same plaintext is found, its ciphertext is returned</param>
			/// <returns>
			/// Ciphertext corresponding to the plaintext inserted.
			/// Null, if tree needs rebalancing (plaintext is not inserted).
			/// </returns>
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

					var newCipher = DivisionHelper(ciphertext, max);
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

					var newCipher = DivisionHelper(min, ciphertext);
					left = new Node(_G, input, newCipher);

					return newCipher;
				}

				throw new InvalidOperationException("Should never be here.");
			}

			/// <summary>
			/// Mirrors <see cref="State"/> corresponding method.
			/// </summary>
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

			/// <summary>
			/// Traverses the nodes in in-order fashion.
			/// Puts plaintext values in result parameter.
			/// </summary>
			/// <param name="result">List to populate with node's plaintext.</param>
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

			private ulong DivisionHelper(ulong min, ulong max)
			{
				ulong diff = max - min;
				ulong add = (diff / 2) + (diff % 2);

				return min + add;
			}
		}
	}
}
