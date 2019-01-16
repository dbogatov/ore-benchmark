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
			public Wrapper(T value) => Value = value;

			public T Value { get; private set; }
		}

		public class Database : Wrapper<Dictionary<byte[], byte[]>>
		{
			public Database(Dictionary<byte[], byte[]> value, int size) : base(value) => Size = size;

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

			public int Size { get; private set; }
		}

		public class Token : Wrapper<(byte[] k1, byte[] k2)>
		{
			public Token((byte[] k1, byte[] k2) value) : base(value) { }

			public int Size { get => 8 * (Value.k1.Length + Value.k2.Length); }
		}

		public class Client : EventHandlers
		{
			private readonly IPRG G;
			private readonly IPRF F;
			private readonly ISymmetric E;

			public Client(byte[] entropy = null)
			{
				G = new PRGFactory(entropy).GetPrimitive();
				F = new PRFFactory().GetPrimitive();
				E = new SymmetricFactory().GetPrimitive();

				SubscribePrimitive(G);
				SubscribePrimitive(F);
				SubscribePrimitive(E);
			}

			public (Database, byte[]) Setup(Dictionary<W, I[]> input, int b = int.MaxValue, int B = 1)
			{
				var key = G.GetBytes(256 / 8);
				var result = new Dictionary<byte[], byte[]>(new Database.ByteArrayComparer());
				var size = 0;

				foreach (var wordIndices in input)
				{
					var word = wordIndices.Key;
					var indices = wordIndices.Value;

					var k1 = F.PRF(key.Take(128 / 8).ToArray(), word.ToBytes());
					var k2 = F.PRF(key.Skip(128 / 8).ToArray(), word.ToBytes());

					for (int c = 0; c < indices.Length; c++)
					{
						var l = F.PRF(k1, BitConverter.GetBytes(c));
						var d = E.Encrypt(k2, indices[c].ToBytes());
						result.Add(l, d);
					}

					size += 8 * word.ToBytes().Length;
					size += 8 * indices[0].ToBytes().Length * (indices.Length > b ? (int)Math.Ceiling(1.0 * indices.Length / B) * B : indices.Length);
				}

				return (new Database(result, size), key);
			}

			public Token Trapdoor(W keyword, byte[] key)
				=> new Token((
					k1: F.PRF(key.Take(128 / 8).ToArray(), keyword.ToBytes()),
					k2: F.PRF(key.Skip(128 / 8).ToArray(), keyword.ToBytes())
				));
		}

		public class Server : EventHandlers
		{
			private readonly Database _database;

			private readonly IPRF F;
			private readonly ISymmetric E;

			/// <summary>
			/// I/O page size in bits.
			/// If set, NodeVisited event will be fired.
			/// </summary>
			public int? PageSize { private get; set; }
			private readonly IPRG _G; // internal

			public Server(Database database, byte[] entropy = null)
			{
				_database = database;

				F = new PRFFactory().GetPrimitive();
				E = new SymmetricFactory().GetPrimitive();

				_G = new PRGFactory(entropy).GetPrimitive();

				SubscribePrimitive(F);
				SubscribePrimitive(E);
			}

			public I[] Search(Token token, Func<byte[], I> decode, int b = int.MaxValue, int B = 1)
			{
				var c = 0;
				var result = new List<I>();

				while (true)
				{
					var dictKey = F.PRF(token.Value.k1, BitConverter.GetBytes(c));
					if (_database.Value.ContainsKey(dictKey))
					{
						var d = _database.Value[dictKey];
						var id = E.Decrypt(token.Value.k2, d);

						result.Add(decode(id));

						c++;
					}
					else
					{
						break;
					}
				}

				if (PageSize.HasValue)
				{
					var totalPages = (int)Math.Ceiling(1.0 * _database.Size / PageSize.Value);
					if (c < b)
					{
						for (int i = 0; i < c; i++)
						{
							OnVisit(_G.Next(0, totalPages));
						}
					}
					else
					{
						for (int i = 0; i < (int)Math.Ceiling(1.0 * c / B); i++)
						{
							OnVisit(_G.Next(0, totalPages));
						}
					}
				}

				return result.ToArray();
			}
		}

	}
}
