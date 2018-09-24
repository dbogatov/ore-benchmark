using System.Collections.Generic;
using ORESchemes.Shared.Primitives.PRG;

namespace Simulation.Protocol.POPE
{
	public class Server : AbsParty
	{
		private readonly IPRG G;
		private Tree<Cipher> _tree;

		public Server(byte[] entropy, int L)
		{
			G = new PRGFactory(entropy).GetPrimitive();

			G.PrimitiveUsed += (prim, impure) => OnPrimitiveUsed(prim, impure);

			var options = new Options<Cipher>
			{
				L = L,
				SetList =
					list =>
					_mediator.SendToClient<HashSet<Cipher>, object>(
						new SetListMessage(list)
					),
				GetSortedList =
					() =>
					_mediator.SendToClient<object, List<Cipher>>(
						new GetSortedListMessage()
					).Unpack(),
				IndexToInsert =
					cipher =>
					_mediator.SendToClient<Cipher, int>(
						new IndexOfResultMessage(cipher)
					).Unpack(),
				IndexOfResult =
					cipher =>
					_mediator.SendToClient<Cipher, int>(
						new IndexOfResultMessage(cipher)
					).Unpack(),
				G = G
			};

			options.NodeVisited += OnNodeVisited;

			_tree = new Tree<Cipher>(options);
		}

		public override IMessage<R> AcceptMessage<Q, R>(IMessage<Q> message)
		{
			switch (message)
			{
				case InsertMessage<Cipher> insert:
					return (IMessage<R>)AcceptMessage(insert);
				case QueryMessage<Cipher> query:
					return (IMessage<R>)AcceptMessage(query);
				default:
					return (IMessage<R>)new FinishMessage();
			}
		}

		/// <summary>
		/// React to the insert request from server
		/// </summary>
		private FinishMessage AcceptMessage(InsertMessage<Cipher> insert)
		{
			_tree.Insert(insert.Unpack());

			return new FinishMessage();
		}

		/// <summary>
		/// React to the search request from server
		/// </summary>
		private QueryResponseMessage AcceptMessage(QueryMessage<Cipher> query)
			=> new QueryResponseMessage(
				_tree.Search(
					query.Unpack().Item1,
					query.Unpack().Item2
				)
			);
	}
}
