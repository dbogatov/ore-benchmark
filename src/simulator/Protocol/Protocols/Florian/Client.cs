using System;
using System.Collections.Generic;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;

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
				int from = GetSearchIndex(query.from, query.to, from: true);
				int to = GetSearchIndex(query.from, query.to, from: false);

				if (from <= to)
				{
					_mediator.SendToServer<Tuple<int, int>, List<string>>(
						new QueryMessage(
							new Tuple<int, int>(from, to)
						)
					);
				}
				else
				{
					int n = _mediator.SendToServer<object, int>(
						new RequestNMessage()
					).Unpack();

					_mediator.SendToServer<Tuple<int, int>, List<string>>(
						new QueryMessage(
							new Tuple<int, int>(0, from)
						)
					);
					_mediator.SendToServer<Tuple<int, int>, List<string>>(
						new QueryMessage(
							new Tuple<int, int>(to, n)
						)
					);
				}
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

		private int InsertValue(int m)
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

			bool nextGreater = false;

			while (l != u)
			{
				int j = (int)Math.Ceiling((double)(l + (u - l) / 2));

				int mPrime = Decrypt(
					_mediator.SendToServer<int, Cipher>(
						new RequestCipherMessage(j)
					).Unpack()
				);

				if (m == mPrime)
				{
					if (G.Next() % 2 == 0)
					{
						l = j + 1;
					}
					else
					{
						u = j;
					}
				}
				else if (m > r)
				{
					if (m > mPrime)
					{
						l = j + 1;
					}
					else
					{
						u = j;
					}
				}
				else
				{
					// m < r
					if (mPrime > r)
					{
						u = j;
					}
					else if (mPrime == r) // TODO ?? 
					{
						if (nextGreater)
						{
							u = j;
						}
						else
						{
							l = j + 1;
						}
					}
					else
					{
						// m < r AND mPrime < r
						if (m < mPrime)
						{
							u = j;
						}
						else
						{
							// m < r AND mPrime < r AND m > mPrime
							l = j + 1;
						}
					}
				}

				nextGreater = mPrime > r;

				// if ((mPrime - r) % UInt32.MaxValue > (m - r) % UInt32.MaxValue)
				// {
				// 	l = j + 1;
				// }
				// else if ((mPrime - r) % UInt32.MaxValue < (m - r) % UInt32.MaxValue)
				// {
				// 	u = j;
				// }
				// else if (m == mPrime)
				// {
				// 	if (G.Next() % 2 == 0)
				// 	{
				// 		l = j + 1;
				// 	}
				// 	else
				// 	{
				// 		u = j;
				// 	}
				// }
				// else
				// {
				// 	throw new InvalidOperationException("Should never happen");
				// }
			}

			return l;
		}

		private int GetSearchIndex(int a, int b, bool from)
		{
			int n = _mediator.SendToServer<object, int>(
				new RequestNMessage()
			).Unpack();

			int l = 0;
			int u = n - 1;

			Cipher c0 = _mediator.SendToServer<int, Cipher>(
				new RequestCipherMessage(0)
			).Unpack();

			Cipher cNMinusOne = _mediator.SendToServer<int, Cipher>(
				new RequestCipherMessage(n - 1)
			).Unpack();

			int r = Decrypt(c0);

			if (r == Decrypt(cNMinusOne) && from)
			{
				r++;
			}

			while (l != u)
			{
				int j = (int)Math.Ceiling((double)(l + (u - l) / 2));

				Cipher cj = _mediator.SendToServer<int, Cipher>(
					new RequestCipherMessage(j)
				).Unpack();

				int m = Decrypt(cj);

				if (
					m - r > a - r ||
					m - r <= b - r
				)
				{
					l = j + 1;
				}
				else
				{
					u = j;
				}
			}

			return from ? l : u;
		}
	}
}
