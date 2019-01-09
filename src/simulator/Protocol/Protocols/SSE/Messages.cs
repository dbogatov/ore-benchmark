using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ORESchemes.CJJKRS.CJJKRS<Simulation.Protocol.SSE.Word, Simulation.Protocol.SSE.Index>;

namespace Simulation.Protocol.SSE
{
	internal class PublishDatabaseMessage : AbsMessage<Database>
	{
		public PublishDatabaseMessage(Database content) : base(content) { }

		public override int GetSize() => _content.Size;
	}

	public class TokensMessage : AbsMessage<Token[]>
	{
		public TokensMessage(Token[] content) : base(content) { }

		public override int GetSize() => _content.Sum(t => t.Size);
	}

	public class EncryptedIndicesMessage : AbsMessage<EncryptedIndices[]>
	{
		public EncryptedIndicesMessage(EncryptedIndices[] content) : base(content) { }

		public override int GetSize() => _content.Sum(i => i.Value.Length);
	}
}
