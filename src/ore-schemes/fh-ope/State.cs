using System;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public class State
	{
		internal IPRG _G;
		private long min;
		private long max;

		private Node _root = null;

		public State(IPRG prg, long min, long max)
		{
			_G = prg;
			this.min = min;
			this.max = max;
		}

		internal long Insert(int plaintext)
		{
			if (_root == null)
			{
				var newCipher = min + (long)Math.Ceiling(0.5 * (max - min));
				_root = new Node(_G, plaintext, newCipher);

				return newCipher;
			}
			else
			{
				return _root.Insert(plaintext, min, max);
			}
		}

		internal int Get(long input)
		{
			if (_root == null)
			{
				throw new InvalidOperationException($"Plaintext {input} was never encrypted.");
			}
			else
			{
				return _root.Get(input);
			}
		}

		private class Node
		{
			private IPRG _G;

			public Node(IPRG prg, int plaintext, long ciphertext)
			{
				_G = prg;
				this.plaintext = plaintext;
				this.ciphertext = ciphertext;
			}

			private Node left = null;
			private Node right = null;

			private int plaintext;
			private long ciphertext;

			public long Insert(int input, long min, long max)
			{
				bool? coin = null;

				if (input == this.plaintext)
				{
					coin = _G.Next() % 2 == 1;
				}

				if (input > this.plaintext || coin.Value == true)
				{
					if (right != null)
					{
						return right.Insert(input, ciphertext, max);
					}
					else
					{
						if (max - ciphertext < 2)
						{
							throw new NotImplementedException("Rebalancing needed");
						}
					}

					var newCipher = ciphertext + (long)Math.Ceiling(0.5 * (max - ciphertext));
					right = new Node(_G, input, newCipher);

					return newCipher;
				}

				if (input < this.plaintext || coin.Value == false)
				{
					if (left != null)
					{
						return left.Insert(input, min, ciphertext);
					}
					else
					{
						if (ciphertext - min < 2)
						{
							throw new NotImplementedException("Rebalancing needed");
						}
					}

					var newCipher = min + (long)Math.Ceiling(0.5 * (ciphertext - min));
					left = new Node(_G, input, newCipher);

					return newCipher;
				}

				throw new InvalidOperationException("Should never be here.");
			}

			public int Get(long input)
			{
				if (input > ciphertext)
				{
					if (right == null)
					{
						throw new InvalidOperationException($"Plaintext {input} was never encrypted.");
					}
					return right.Get(input);
				}
				else if (input < ciphertext)
				{
					if (left == null)
					{
						throw new InvalidOperationException($"Plaintext {input} was never encrypted.");
					}
					return left.Get(input);
				}

				return plaintext;
			}
		}
	}
}
