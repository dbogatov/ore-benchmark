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

namespace CJJKRS
{
	public interface IWord : global::ORESchemes.Shared.Primitives.TSet.IWord { }
	public interface IIndex : IByteable { }

	public static class CJJKRS<W, I>
		where W : IWord
		where I : IIndex
	{
		public abstract class Wrapper<T>
		{
			public T Value { get; set; }
		}

		public class Token : Wrapper<byte[]> { }
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

				SubscribePrimitive(G);
				SubscribePrimitive(F);
				SubscribePrimitive(E);
				SubscribePrimitive(T);

				_ks = G.GetBytes(128 / 8);
			}

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

			public Token Trapdoor(W keyword)
				=> new Token { Value = T.GetTag(_kt, (ORESchemes.Shared.Primitives.TSet.IWord)keyword) };

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

			public Server(Database database)
			{
				_database = database;

				T = new TSetFactory().GetPrimitive();

				SubscribePrimitive(T);

				T.NodeVisited += new NodeVisitedEventHandler(OnVisit);
			}

			public EncryptedIndices Search(Token token)
				=> new EncryptedIndices { Value = T.Retrive(_database.Value, token.Value) };
		}

	}
}
