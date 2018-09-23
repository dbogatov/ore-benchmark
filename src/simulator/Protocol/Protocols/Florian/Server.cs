using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.Florian
{
	public class Server : AbsParty
	{
		private readonly IPRG G;
		private List<Tuple<Cipher, string>> _structure;
		private readonly int _blockSize;

		public Server(byte[] entropy, int blockSize)
		{
			G = new PRGFactory(entropy).GetPrimitive();

			G.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);

			_structure = new List<Tuple<Cipher, string>>();

			_blockSize = blockSize;
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case RequestNMessage requestN:
					return (IMessage<R>)new ResponseNMessage(_structure.Count);
				case RequestCipherMessage requestCipher:
					// Set block hash a derivative of first element (so if shift, different hash) and block number where element requested
					// The idea is that middle element should be cached if no shifts occurred
					OnNodeVisited(_structure[0].Item1.GetHashCode() * 2 + (requestCipher.Unpack() / _blockSize) * 3);
					return (IMessage<R>)new ResponseCipherMessage(_structure[requestCipher.Unpack()].Item1);
				case InsertMessage insert:
					return (IMessage<R>)AcceptMessage(insert);
				case QueryMessage query:
					return (IMessage<R>)AcceptMessage(query);
				default:
					return (IMessage<R>)new FinishMessage();
			}
		}

		/// <summary>
		/// React to the search query request from client
		/// </summary>
		private QueryResponseMessage AcceptMessage(QueryMessage query)
		{
			// When query is executed, all return blocks are accessed
			for (int i = query.Unpack().Item1 / _blockSize; i <= query.Unpack().Item2 / _blockSize; i++)
			{
				OnNodeVisited(_structure[0].Item1.GetHashCode() * 2 + i * 3);
			}

			return new QueryResponseMessage(
				_structure
					.Skip(query.Unpack().Item1)
					.Take(query.Unpack().Item2 - query.Unpack().Item1)
					.Select(e => e.Item2)
					.ToList()
			);
		}

		/// <summary>
		/// React to the insert request from client
		/// </summary>
		private FinishMessage AcceptMessage(InsertMessage insert)
		{
			_structure.Insert(
				insert.Unpack().location,
				new Tuple<Cipher, string>(
					insert.Unpack().index,
					insert.Unpack().value
				)
			);

			// Rotate
			int n = _structure.Count;
			int s = G.Next(0, n);

			Tuple<Cipher, string>[] @new = new Tuple<Cipher, string>[_structure.Count];
			for (int i = 0; i < n; i++)
			{
				@new[(i + s) % n] = _structure[i];
			}
			_structure = @new.ToList();

			// When shift happens, all blocks are accessed
			for (int i = 0; i <= _structure.Count / _blockSize; i++)
			{
				OnNodeVisited(_structure[0].Item1.GetHashCode() * 2 + i * 3);
			}

			return new FinishMessage();
		}

		/// <summary>
		/// Helper method visible only the test assembly
		/// </summary>
		/// <param name="decode">Lambda used to decrypt the ciphertext</param>
		/// <returns>True, if internal structure is valid (sorted)</returns>
		internal bool ValidateStructure(Func<Cipher, int> decode)
		{
			var structure = _structure.Select(e => decode(e.Item1)).ToArray();
			int n = structure.Length;

			int jumps = 0;
			for (int i = 0; i < n; i++)
			{
				if (structure[i % n] > structure[(i + 1) % n])
				{
					jumps++;
				}
			}

			return jumps <= 1;
		}
	}
}
