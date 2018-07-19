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

		public Server(byte[] entropy)
		{
			G = new PRGFactory(entropy).GetPrimitive();

			_structure = new List<Tuple<Cipher, string>>();
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case RequestNMessage requestN:
					return (IMessage<R>)new ResponseNMessage(_structure.Count);
				case RequestCipherMessage requestCipher:
					return (IMessage<R>)new ResponseCipherMessage(_structure[requestCipher.Unpack()].Item1);
				case InsertMessage insert:
					return (IMessage<R>)AcceptMessage(insert);
				case QueryMessage query:
					return (IMessage<R>)new QueryResponseMessage(
						_structure
							.Skip(query.Unpack().Item1)
							.Take(query.Unpack().Item2 - query.Unpack().Item1)
							.Select(e => e.Item2)
							.ToList()
						);
				default:
					return (IMessage<R>)new FinishMessage();
			}
		}

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

			// List<Tuple<Cipher, string>> @new = new List<Tuple<Cipher, string>>(_structure.Count);
			Tuple<Cipher, string>[] @new = new Tuple<Cipher, string>[_structure.Count];
			for (int i = 0; i < n; i++)
			{
				@new[(i + s) % n] = _structure[i];
			}
			_structure = @new.ToList();

			return new FinishMessage();
		}
	}
}
