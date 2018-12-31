﻿using System;
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
	public interface IWord : IByteable { }
	public interface IIndex : IByteable { }

	// public class Key
	// {
	// 	public byte[] Ks { get; set; }
	// 	public byte[] Kt { get; set; }
	// }

	public class Database : TSetStructure { }

	public class Token
	{
		public byte[] STag { get; set; }
	}

	public class EncryptedIndices
	{
		public BitArray[] Indices { get; set; }
	}

	// TODO events
	public class Client
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
		}

		public Database Setup(Dictionary<IWord, IIndex[]> input)
		{
			var TInput = new Dictionary<ORESchemes.Shared.Primitives.TSet.IWord, BitArray[]>();

			foreach (var wordIndices in input)
			{
				var word = wordIndices.Key;
				var indices = wordIndices.Value;

				var Ke = F.PRF(_ks, word.ToBytes());


				for (int i = indices.Length; i > 0; i--)
				{
					// TODO PRP event
					int j = G.Next(0, i);

					IIndex temp = indices[i];
					indices[i] = indices[j];
					indices[j] = temp;
				}

				var t = indices.Select(ind => new BitArray(E.Encrypt(Ke, ind.ToBytes()))).ToArray();

				TInput[(ORESchemes.Shared.Primitives.TSet.IWord)word] = t;
			}

			(var TSet, var Kt) = T.Setup(TInput);

			_kt = Kt;

			return (Database)TSet;
		}

		public Token Trapdoor(IWord keyword)
		{
			return new Token { STag = T.GetTag(_kt, (ORESchemes.Shared.Primitives.TSet.IWord)keyword) };
		}

		public IIndex[] Decrypt(EncryptedIndices encrypted, IWord keyword, Func<byte[], IIndex> decode)
		{
			var Ke = F.PRF(_ks, keyword.ToBytes());

			var decrypted = encrypted.Indices.Select(enc => decode(E.Decrypt(Ke, enc.ToBytes()))).ToArray();
			
			return decrypted;
		}
	}

	public class Server
	{
		private readonly Database _database;
		private readonly ITSet T;

		public Server(Database database)
		{
			_database = database;

			T = new TSetFactory().GetPrimitive();
		}

		public EncryptedIndices Search(Token token)
		{
			return new EncryptedIndices { Indices = T.Retrive(_database, token.STag) };
		}
	}
}
