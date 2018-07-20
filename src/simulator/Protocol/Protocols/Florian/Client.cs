using System;
using System.Collections.Generic;
using System.Linq;
// using System.Runtime.CompilerServices;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;

// [assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.Florian
{
	public class Client : AbsClient
	{
		private readonly ISymmetric E;
		private readonly IPRG G;
		private readonly byte[] _key;

		public Client(byte[] entropy)
		{
			E = new SymmetricFactory().GetPrimitive();
			G = new PRGFactory(entropy).GetPrimitive();

			_key = G.GetBytes(128 / 8);

			OnClientStorage(_key.Length * 8);
		}

		public override void RunHandshake() { }

		public override void RunConstruction(List<Record> input)
		{
			foreach (var record in input)
			{
				int l = InsertValue(record.index);

				_mediator.SendToServer<InsertContent, object>(
					new InsertMessage(
						new InsertContent
						{
							value = record.value,
							index = Encrypt(record.index),
							location = l
						}
					)
				);
			}
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				Search(query);
			}
		}

		internal List<string> Search(RangeQuery query)
		{
			int from = GetSearchIndex(query.from, query.to, from: true);
			int to = GetSearchIndex(query.from, query.to, from: false);

			if (from <= to)
			{
				return _mediator.SendToServer<Tuple<int, int>, List<string>>(
					new QueryMessage(
						new Tuple<int, int>(from, to)
					)
				).Unpack();
			}
			else
			{
				int n = _mediator.SendToServer<object, int>(
					new RequestNMessage()
				).Unpack();

				return _mediator.SendToServer<Tuple<int, int>, List<string>>(
					new QueryMessage(
						new Tuple<int, int>(0, to)
					)
				).Unpack()
				.Concat(
					_mediator.SendToServer<Tuple<int, int>, List<string>>(
						new QueryMessage(
							new Tuple<int, int>(from, n)
						)
					).Unpack()
				).ToList();
			}
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			OnClientStorage(message.GetSize());

			return (IMessage<R>)new FinishMessage();
		}

		private Cipher Encrypt(int input)
			=> new Cipher { encrypted = E.Encrypt(_key, BitConverter.GetBytes(input)) };

		private int Decrypt(Cipher input)
			=> BitConverter.ToInt32(E.Decrypt(_key, input.encrypted), 0);

		private int InsertValue(int value)
		{
			int n = _mediator.SendToServer<object, int>(
				new RequestNMessage()
			).Unpack();

			int l = 0;
			int u = n;

			if (n == 0)
			{
				return 0;
			}

			int r = Decrypt(
				_mediator.SendToServer<int, Cipher>(
					new RequestCipherMessage(0)
				).Unpack()
			);

			int trueLocation = -1;

			while (l != u)
			{
				int j = (int)Math.Ceiling((double)(l + (u - l) / 2));

				int middle = Decrypt(
					_mediator.SendToServer<int, Cipher>(
						new RequestCipherMessage(j)
					).Unpack()
				);

				if (Modulo(value - r) > Modulo(middle - r))
				{
					// go right
					l = j + 1;
				}
				else if (Modulo(value - r) < Modulo(middle - r))
				{
					// go left
					u = j;
				}
				else if (value == middle)
				{
					// Bug in the original paper
					trueLocation = j;

					if (G.Next() % 2 == 0)
					{
						// go right
						l = j + 1;
					}
					else
					{
						// go left
						u = j;
					}
				}
				else
				{
					throw new InvalidOperationException("Should never happen");
				}
			}

			return trueLocation != -1 ? trueLocation : l;
		}

		private int GetSearchIndex(int a, int b, bool from)
		{
			int n = _mediator.SendToServer<object, int>(
				new RequestNMessage()
			).Unpack();

			int l = 0;
			int u = n;

			int r = Decrypt(
				_mediator.SendToServer<int, Cipher>(
					new RequestCipherMessage(0)
				).Unpack()
			);

			int nMinusOne = Decrypt(
				_mediator.SendToServer<int, Cipher>(
					new RequestCipherMessage(n - 1)
				).Unpack()
			);

			// a <= r was not in original paper
			// has to put it here to fix a bug
			// In particular the following failed
			// structure = [72, 72, 73, 73, .... , 72]
			// and looking for a = 73, from = true
			if (r == nMinusOne && from && a <= r)
			{
				r++;
			}

			while (l != u)
			{
				int j = (int)Math.Ceiling((double)(l + (u - l) / 2));

				int middle = Decrypt(
					_mediator.SendToServer<int, Cipher>(
						new RequestCipherMessage(j)
					).Unpack()
				);

				if (
					(Modulo(a - r) > Modulo(middle - r) && from) ||
					(Modulo(b - r) >= Modulo(middle - r) && !from)
				)
				{
					// go right
					l = j + 1;
				}
				else
				{
					// go left
					u = j;
				}
			}

			return from ? l : u;
		}
		private long Modulo(int a) => a >= 0 ? a : UInt32.MaxValue + a;

		internal Func<Cipher, int> ExportDecryption() => Decrypt;
	}
}
