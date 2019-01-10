using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;
using ORESchemes.Shared.Primitives.TSet;

namespace ORESchemes.CJJKRS
{
	/// <summary>
	/// A byteable keyword for SSE
	/// </summary>
	public interface IWord : global::ORESchemes.Shared.Primitives.TSet.IWord { }
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
	public static class CJJKRS<W, I>
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

		public class Token : Wrapper<byte[]>
		{
			public int Size { get => Value.Count() * 8; }
		}
		public class EncryptedIndices : Wrapper<BitArray[]> { }
		public class Database : Wrapper<TSetStructure>
		{
			public int Size { get => ((TSetStructure)Value).Size; }
		}

		public class Client : EventHandlers
		{
			private readonly IPRG G;
			private readonly IPRF F;
			private readonly ISymmetric E;
			private readonly ITSet T;

			private readonly byte[] _ks;
			private byte[] _kt;

			public Client(byte[] entropy = null)
			{
				G = new PRGFactory(entropy).GetPrimitive();
				F = new PRFFactory().GetPrimitive();
				E = new SymmetricFactory().GetPrimitive();
				T = new TSetFactory(G.GetBytes(128 / 8)).GetPrimitive();

				_ks = G.GetBytes(128 / 8);
				OnPrimitive(Primitive.PRG);

				G.PrimitiveUsed += new PrimitiveUsageEventHandler((prim, impure) => OnPrimitive(prim, true));
				T.PrimitiveUsed += new PrimitiveUsageEventHandler((prim, impure) => OnPrimitive(prim, prim != Primitive.TSet));
				SubscribePrimitive(F);
				SubscribePrimitive(E);
			}

			/// <summary>
			/// EDBSetup(DB) routine from https://eprint.iacr.org/2013/169.pdf
			/// </summary>
			/// <param name="input">A keyword to indices map</param>
			/// <returns>An SSE encrypted database</returns>
			public Database Setup(Dictionary<W, I[]> input)
			{
				var TInput = new Dictionary<ORESchemes.Shared.Primitives.TSet.IWord, BitArray[]>();

				foreach (var wordIndices in input)
				{
					var word = wordIndices.Key;
					var indices = wordIndices.Value;

					var Ke = F.PRF(_ks, word.ToBytes());

					for (int i = indices.Length - 1; i >= 0; i--)
					{
						int j = G.Next(0, i);

						I temp = indices[i];
						indices[i] = indices[j];
						indices[j] = temp;
					}
					OnPrimitive(Primitive.PRP);

					var t = indices.Select(ind => new BitArray(E.Encrypt(Ke, ind.ToBytes()))).ToArray();

					TInput[word] = t;
				}

				(var TSet, var Kt) = T.Setup(TInput);

				_kt = Kt;

				return new Database { Value = TSet };
			}

			/// <summary>
			/// A trapdoor function, a part from Search protocol in https://eprint.iacr.org/2013/169.pdf
			/// </summary>
			/// <param name="keyword">A keyword for which to generate token</param>
			/// <returns>A token for server to search on</returns>
			public Token Trapdoor(W keyword)
				=> new Token { Value = T.GetTag(_kt, (ORESchemes.Shared.Primitives.TSet.IWord)keyword) };

			/// <summary>
			/// Decryption procedure, a part from Search protocol in https://eprint.iacr.org/2013/169.pdf
			/// </summary>
			/// <param name="encrypted">Encrypted indices returned by Server's search procedure</param>
			/// <param name="keyword">A keyword for which the search protocol was run</param>
			/// <param name="decode">A decoding routine that converts bytes representation of indices to user's type</param>
			/// <returns>An array of (user's type) indices for given keyword</returns>
			public I[] Decrypt(EncryptedIndices encrypted, W keyword, Func<byte[], I> decode)
			{
				var Ke = F.PRF(_ks, keyword.ToBytes());

				var decrypted = encrypted.Value.Select(enc => decode(E.Decrypt(Ke, enc.ToBytes()))).ToArray();

				return decrypted;
			}
		}

		public class Server : EventHandlers
		{
			private readonly Database _database;
			private readonly ITSet T;

			/// <summary>
			/// I/O page size in bits.
			/// If set, NodeVisited event will be fired.
			/// </summary>
			public int? PageSize { set { T.PageSize = value; } }

			public Server(Database database)
			{
				_database = database;

				T = new TSetFactory().GetPrimitive();

				SubscribePrimitive(T);

				T.NodeVisited += new NodeVisitedEventHandler(OnVisit);
			}

			/// <summary>
			/// Server's part of Search protocol in https://eprint.iacr.org/2013/169.pdf
			/// </summary>
			/// <param name="token">A client-generated search token</param>
			/// <returns>An array of encrypted indices</returns>
			public EncryptedIndices Search(Token token)
				=> new EncryptedIndices { Value = T.Retrive(_database.Value, token.Value) };
		}

	}
}
