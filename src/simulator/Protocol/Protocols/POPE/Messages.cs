using System.Collections.Generic;
using System.Linq;

namespace Simulation.Protocol.POPE
{
	internal class SetListMessage : AbsMessage<HashSet<Cipher>>
	{
		public SetListMessage(HashSet<Cipher> content) : base(content) { }

		public override int GetSize() => _content.Sum(c => c == null ? 0 : c.GetSize());
	}

	internal class GetSortedListMessage : RequestMessage { }

	internal class SortedListResponseMessage : AbsMessage<List<Cipher>>
	{
		public SortedListResponseMessage(List<Cipher> content) : base(content) { }

		public override int GetSize() => _content.Sum(c => c == null ? 0 : c.GetSize());
	}

	internal class IndexOfResultMessage : SizeableMessage<Cipher>
	{
		public IndexOfResultMessage(Cipher content) : base(content) { }
	}
	
	internal class IndexResponseMessage : AbsMessage<int>
	{
		public IndexResponseMessage(int content) : base(content) { }

		public override int GetSize() => sizeof(int) * 8;
	}
}
