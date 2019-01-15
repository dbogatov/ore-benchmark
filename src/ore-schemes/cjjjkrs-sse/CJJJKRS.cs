using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;

namespace ORESchemes.CJJJKRS
{
	/// <summary>
	/// A byteable keyword for SSE
	/// </summary>
	public interface IWord : IByteable { }
	/// <summary>
	/// A byteable index for SSE
	/// </summary>
	public interface IIndex : IByteable { }

	/// <summary>
	/// A namespace wrapper that ensures that the same generic types are provided
	/// to Client and Server
	/// </summary>
	/// <typeparam name="W">Word type</typeparam>
	/// <typeparam name="I">Index type</typeparam>
	public static class CJJJKRS<W, I>
		where W : IWord
		where I : IIndex
	{
		/// <summary>
		/// Convenience wrapper class to wrap around TSet types
		/// </summary>
		/// <typeparam name="T">Wrapped type</typeparam>
		public abstract class Wrapper<T>
		{
			public T Value { get; set; }
		}

		public class Database : Wrapper<Dictionary<byte[], byte[]>>
		{
			/// <summary>
			/// Needed for correct behavior of dictionary with byte array key
			/// </summary>
			public class ByteArrayComparer : EqualityComparer<byte[]>
			{
				// https://stackoverflow.com/a/30353296/1644554
				public override bool Equals(byte[] first, byte[] second)
				{
					if (first == null || second == null)
					{
						return first == second;
					}
					if (ReferenceEquals(first, second))
					{
						return true;
					}
					if (first.Length != second.Length)
					{
						return false;
					}
					return first.SequenceEqual(second);
				}
				public override int GetHashCode(byte[] obj)
					=> obj == null ? 0 : obj.ProperHashCode();
			}

			public int Size { get => 8 * Value.Sum(kvp => kvp.Key.Length + kvp.Value.Length); }
		}

		public class Token : Wrapper<(byte[] k1, byte[] k2)>
		{
			public int Size { get => 8 * (Value.k1.Length + Value.k2.Length); }
		}

		public class Client : EventHandlers
		{
			private readonly IPRG G;
			private readonly IPRF F;
			private readonly ISymmetric E;

			private readonly int _b;
			private readonly int _B;

			public Client(int b = int.MaxValue, int B = 1, byte[] entropy = null)
			{
				_b = b;
				_B = B;

				G = new PRGFactory(entropy).GetPrimitive();
				F = new PRFFactory().GetPrimitive();
				E = new SymmetricFactory().GetPrimitive();

				SubscribePrimitive(G);
				SubscribePrimitive(F);
				SubscribePrimitive(E);
			}

			public (Database, byte[]) Setup(Dictionary<W, I[]> input)
			{
				var key = G.GetBytes(256 / 8);
				var result = new Dictionary<byte[], byte[]>(new Database.ByteArrayComparer());

				foreach (var wordIndices in input)
				{
					var word = wordIndices.Key;
					var indices = wordIndices.Value;

					var k1 = F.PRF(key.Take(128 / 8).ToArray(), word.ToBytes());
					var k2 = F.PRF(key.Skip(128 / 8).ToArray(), word.ToBytes());

					if (indices.Length < _b)
					{
						for (int c = 0; c < indices.Length; c++)
						{
							var l = F.PRF(k1, BitConverter.GetBytes(c));
							var d = E.Encrypt(k2, indices[c].ToBytes());
							result.Add(l, d);
						}
					}
					else
					{
						for (int c = 0; c < indices.Length; c += _B)
						{
							var l = F.PRF(k1, BitConverter.GetBytes(c));
							var bucket = indices.Skip(c).Take(_B).Select(i => i.ToBytes());
							if (bucket.Count() < _B)
							{
								bucket = bucket.Concat(
									Enumerable
										.Range(0, _B - bucket.Count())
										.Select(_ =>
											Enumerable
												.Repeat((byte)0x00, 128 / 8)
												.ToArray()
										)
								);
							}

							var d = E.Encrypt(k2, bucket.SelectMany(b => b).ToArray());
							result.Add(l, d);
						}
					}

				}

				return (new Database { Value = result }, key);
			}

			public Token Trapdoor(W keyword, byte[] key)
				=> new Token
				{
					Value = (
						k1: F.PRF(key.Take(128 / 8).ToArray(), keyword.ToBytes()),
						k2: F.PRF(key.Skip(128 / 8).ToArray(), keyword.ToBytes())
					)
				};
		}

		public class Server : EventHandlers
		{
			private readonly Database _database;
			private readonly IPRF F;
			private readonly ISymmetric E;

			private readonly int _B;

			/// <summary>
			/// I/O page size in bits.
			/// If set, NodeVisited event will be fired.
			/// </summary>
			public int? PageSize { private get; set; }

			public Server(Database database, int B = 1)
			{
				_database = database;
				_B = B;

				F = new PRFFactory().GetPrimitive();
				E = new SymmetricFactory().GetPrimitive();

				SubscribePrimitive(F);
				SubscribePrimitive(E);
			}

			public I[] Search(Token token, Func<byte[], I> decode)
			{
				var c = 0;
				var result = new List<I>();

				while (true)
				{
					var dictKey = F.PRF(token.Value.k1, BitConverter.GetBytes(c));
					if (_database.Value.ContainsKey(dictKey))
					{
						var d = _database.Value[dictKey];
						var ids = E.Decrypt(token.Value.k2, d);
						if (d.Length == (2 * 128 / 8)) // IV + cipher
						{
							result.Add(decode(ids));
						}
						else
						{
							for (int i = 0; i < _B; i++)
							{
								var id = ids.Skip(i * (128 / 8)).Take(_B).ToArray();
								if (!id.All(b => b == 0x00))
								{
									result.Add(decode(id));
								}
								else
								{
									break;
								}
							}

						}

						c++;
					}
					else
					{
						break;
					}
				}

				return result.ToArray();
			}
		}

	}
}
