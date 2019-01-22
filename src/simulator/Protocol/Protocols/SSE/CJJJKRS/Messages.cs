using System.Linq;
using static Crypto.CJJJKRS.Scheme<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE.CJJJKRS
{
	internal class PublishDatabaseMessage : AbsMessage<(Database db, int b, int B)>
	{
		public PublishDatabaseMessage((Database db, int b, int B) content) : base(content) { }

		public override int GetSize() => _content.db.Size;
	}

	public class TokensMessage : AbsMessage<Token[]>
	{
		public TokensMessage(Token[] content) : base(content) { }

		public override int GetSize() => _content.Sum(t => t.Size);
	}

	public class ResultMessage : AbsMessage<Index[]>
	{
		public ResultMessage(Index[] content) : base(content) { }

		public override int GetSize() => 0; // result is inherent and no false positives
	}
}
