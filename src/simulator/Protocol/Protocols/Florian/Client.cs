using System;
using System.Collections.Generic;
using System.Linq;
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

			G.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			E.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);

			OnClientStorage(_key.Length * 8);
		}

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

				OnQueryCompleted();
			}
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				Search(query);

				OnQueryCompleted();
			}
		}

		/// <summary>
		/// Returns a result for a single search
		/// </summary>
		/// <param name="query">Query object containing requested endpoints</param>
		/// <returns>A list of strings - results</returns>
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

		/// <summary>
		/// Helper that encrypts a plaintext
		/// </summary>
		private Cipher Encrypt(int input)
			=> new Cipher { encrypted = E.Encrypt(_key, BitConverter.GetBytes(input)) };

		/// <summary>
		/// Helper that decrypts a ciphertext
		/// </summary>
		private int Decrypt(Cipher input)
			=> BitConverter.ToInt32(E.Decrypt(_key, input.encrypted), 0);

		/// <summary>
		/// Interactively talks to server and returns the index where given value must be inserted
		/// </summary>
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
			}

			return trueLocation != -1 ? trueLocation : l;
		}

		/// <summary>
		/// Interactively talks to server to find indices for query endpoints
		/// </summary>
		/// <param name="a">Left query endpoint</param>
		/// <param name="b">Right query endpoint</param>
		/// <param name="from">True, if index for left endpoint requested, false for right endpoint</param>
		/// <returns>The index of an endpoint</returns>
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

		/// <summary>
		/// Helper that computes mathematically correct modulo -
		/// one that lives between 0 and a - 1
		/// </summary>
		private long Modulo(int a) => a >= 0 ? a : UInt32.MaxValue + a;

		/// <summary>
		/// Helper method visible only to the test assembly.
		/// Needed for proper testing
		/// </summary>
		internal Func<Cipher, int> ExportDecryption() => Decrypt;
	}
}
