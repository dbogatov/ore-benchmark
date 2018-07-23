using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.Symmetric;

namespace Simulation.Protocol.POPE
{
	public class Client : AbsClient
	{
		private readonly ISymmetric E;
		private readonly IPRG G;
		private readonly byte[] _key;

		private List<Cipher> _workingList;
		private List<int> _decrypted;

		public Client(byte[] entropy)
		{
			E = new SymmetricFactory().GetPrimitive();
			G = new PRGFactory(entropy).GetPrimitive();

			_key = G.GetBytes(128 / 8);

			G.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);
			E.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);

			OnClientStorage(_key.Length * 8);
		}

		public override void RunHandshake() { }

		public override void RunConstruction(List<Record> input)
		{
			foreach (var record in input)
			{
				_mediator.SendToServer<EncryptedRecord<Cipher>, object>(
					new InsertMessage<Cipher>(
						new EncryptedRecord<Cipher>
						{
							cipher = Encrypt(record.index),
							value = record.value
						}
					)
				);
			}
		}

		public override void RunSearch(List<RangeQuery> input)
		{
			foreach (var query in input)
			{
				_mediator.SendToServer<Tuple<Cipher, Cipher>, List<string>>(
					new QueryMessage<Cipher>(
						new Tuple<Cipher, Cipher>(
							Encrypt(query.from),
							Encrypt(query.to)
						)
					)
				);
			}
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			OnClientStorage(message.GetSize());

			switch (message)
			{
				case SetListMessage setList:
					_workingList = setList.Unpack().OrderBy(Decrypt).ToList();
					_decrypted = _workingList.Select(Decrypt).ToList();
					break;
				case GetSortedListMessage getSorted:
					return (IMessage<R>)new SortedListResponseMessage(_workingList);
				case IndexOfResultMessage ofResult:
					var decrypted = Decrypt(ofResult.Unpack());
					for (int i = 0; i < _decrypted.Count; i++)
					{
						if (decrypted <= _decrypted[i])
						{
							return (IMessage<R>)new IndexResponseMessage(i);
						}
					}
					throw new InvalidOperationException("Should never be here");
				default:
					return (IMessage<R>)new FinishMessage();
			}

			return (IMessage<R>)new FinishMessage();
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
			=> input == null ? Int32.MaxValue : BitConverter.ToInt32(E.Decrypt(_key, input.encrypted), 0);

		/// <summary>
		/// Helper method visible only to the test assembly.
		/// Needed for proper testing
		/// </summary>
		internal Func<Cipher, int> ExportDecryption() => Decrypt;

		/// <summary>
		/// Helper method visible only to the test assembly.
		/// Needed for proper testing
		/// </summary>
		internal Func<int, Cipher> ExportEncryption() => Encrypt;
	}
}
