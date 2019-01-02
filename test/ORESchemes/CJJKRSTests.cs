using System;
using System.Collections.Generic;
using System.Text;
using CJJKRS;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class CJJKRS
	{
		public class StringWord : IWord
		{
			public string Value { get; set; }

			public byte[] ToBytes() => Encoding.Default.GetBytes(Value);

			public override int GetHashCode() => Value.GetHashCode();
		}

		public class NumericIndex : IIndex
		{
			public int Value { get; set; }

			public byte[] ToBytes() => BitConverter.GetBytes(Value);

			public override bool Equals(object obj) => Value.Equals(obj);

			public override int GetHashCode() => Value.GetHashCode();
		}

		private readonly Client _client;
		private Server _server;

		static readonly int SEED = 123456;
		private readonly Random G = new Random(SEED);

		public CJJKRS()
		{
			byte[] entropy = new byte[128 / 8];

			_client = new Client(entropy);
		}

		[Fact]
		public void NoExceptions()
		{
			var input = new Dictionary<IWord, IIndex[]> {
				{
					new StringWord { Value = "Dmytro" },
					new NumericIndex[] {
						new NumericIndex { Value = 21 },
						new NumericIndex { Value = 05 }
					}
				},
				{
					new StringWord { Value = "Alex" },
					new NumericIndex[] {
						new NumericIndex { Value = 26 },
						new NumericIndex { Value = 10 }
					}
				}
			};

			var database = _client.Setup(input);

			_server = new Server(database);

			// Search protocol
			var keyword = new StringWord { Value = "Dmytro" };
			var token = _client.Trapdoor(keyword);
			var encrypted = _server.Search(token);
			var result = _client.Decrypt(encrypted, keyword, e => new NumericIndex { Value = BitConverter.ToInt32(e, 0) });
		}
	}
}
