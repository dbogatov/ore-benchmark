using System;
using System.Collections.Generic;

namespace Simulation.Protocol.Florian
{
	internal class InsertMessage : SizeableMessage<InsertContent>
	{
		public InsertMessage(InsertContent content) : base(content) { }
	}

	internal class RequestNMessage : RequestMessage { }

	internal class ResponseNMessage : AbsMessage<int>
	{
		public ResponseNMessage(int content) : base(content) { }

		public override int GetSize() => sizeof(int) * 8;
	}

	internal class RequestCipherMessage : AbsMessage<int>
	{
		public RequestCipherMessage(int content) : base(content) { }

		public override int GetSize() => sizeof(int) * 8;
	}

	internal class ResponseCipherMessage : SizeableMessage<Cipher>
	{
		public ResponseCipherMessage(Cipher content) : base(content) { }
	}

	internal class QueryMessage : AbsMessage<Tuple<int, int>>
	{
		public QueryMessage(Tuple<int, int> content) : base(content) { }

		public override int GetSize() => 2 * sizeof(int) * 8;
	}
}
