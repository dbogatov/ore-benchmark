using System.Collections.Generic;
using System.Text;
using CJJKRSScheme = ORESchemes.CJJKRS.CJJKRS<Packages.Program.StringWord, Packages.Program.NumericIndex>;
using CJJJKRSScheme = ORESchemes.CJJJKRS.CJJJKRS<Packages.Program.StringWord, Packages.Program.NumericIndex>;
using System;

namespace Packages
{
	partial class Program
	{
		public class StringWord : ORESchemes.CJJKRS.IWord, ORESchemes.CJJJKRS.IWord
		{
			public string Value { get; set; }

			public byte[] ToBytes() => Encoding.Default.GetBytes(Value);

			public override int GetHashCode() => Value.GetHashCode();
		}

		public class NumericIndex : ORESchemes.CJJKRS.IIndex, ORESchemes.CJJJKRS.IIndex
		{
			public int Value { get; set; }

			public byte[] ToBytes() => BitConverter.GetBytes(Value);

			public override bool Equals(object obj) => Value.Equals(obj);

			public override int GetHashCode() => Value.GetHashCode();

			static public NumericIndex Decode(byte[] encoded) => new NumericIndex { Value = BitConverter.ToInt32(encoded, 0) };
		}

		static private readonly Dictionary<StringWord, NumericIndex[]> _sseInput =
			new Dictionary<StringWord, NumericIndex[]> {
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

		static private void CJJKRS()
		{
			var client = new CJJKRSScheme.Client();

			var database = client.Setup(_sseInput);

			var server = new CJJKRSScheme.Server(database);

			// Search protocol
			var keyword = new StringWord { Value = "Dmytro" };
			var token = client.Trapdoor(keyword);
			var encrypted = server.Search(token);

			var result = client.Decrypt(encrypted, keyword, NumericIndex.Decode);
			// contains 21 and 05
		}

		static private void CJJJKRS()
		{
			var client = new CJJJKRSScheme.Client();

			(var database, var key) = client.Setup(_sseInput);

			var server = new CJJJKRSScheme.Server(database);

			// Search protocol
			var keyword = new StringWord { Value = "Dmytro" };
			var token = client.Trapdoor(keyword, key);

			var result = server.Search(token, NumericIndex.Decode);
			// contains 21 and 05
		}
	}
}
