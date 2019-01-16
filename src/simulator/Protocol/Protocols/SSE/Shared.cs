using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORESchemes.Shared;

namespace Simulation.Protocol.SSE
{
	internal interface ISSEClient
	{
		List<Index> Search(int from, int to);
	}

	internal interface ISSEProtocol
	{
		ISSEClient ExposeClient();

		void RunConstructionProtocol(List<Record> input);

		void RunHandshake();

		void RunQueryProtocol(List<RangeQuery> input);
	}

	public static class Utility
	{
		public static Dictionary<Word, Index[]> InputToDatabase(List<Record> input)
		{
			// generate keyword - index pairs
			var pairs = input
				.DistinctBy(r => r.index)
				.Select(
					record =>
						Cover
							.Path(record.index.ToUInt())
							.Select(
								keyword => (
									index: new Index { Value = record.value },
									word: new Word { Value = keyword }
								)
							)
				)
				.SelectMany(l => l);

			// invert index - generate a hash table where key is a keyword and
			// value is a list of indices
			var database = pairs
				.GroupBy(
					pair => pair.word,
					pair => pair.index
				)
				.ToDictionary(
					group => group.Key,
					group => group.ToArray()
				);

			return database;
		}
	}

	public class Word : ORESchemes.CJJKRS.IWord, ORESchemes.CJJJKRS.IWord
	{
		public (BitArray, int) Value { get; set; }

		/// <summary>
		/// BitArray will be given by Cover.BRC method, so all BitArray values
		/// will be of equal length (32 bits). Therefore, this method will output
		/// unique bytes representation of the tuple.
		/// </summary>
		public byte[] ToBytes()
			=> Value.Item1.ToBytes().Concat(BitConverter.GetBytes(Value.Item2)).ToArray();

		public override int GetHashCode() => Value.Item1.ProperHashCode() * 41 + Value.Item2.GetHashCode() * 37;

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			return
				Value.Item1.IsEqualTo(((Word)obj).Value.Item1) &&
				Value.Item2 == ((Word)obj).Value.Item2;
		}
	}

	public class Index : ORESchemes.CJJKRS.IIndex, ORESchemes.CJJJKRS.IIndex
	{
		public string Value { get; set; }

		public byte[] ToBytes()
			=> Encoding.Default.GetBytes(Value);

		public static Index FromBytes(byte[] bytes)
			=> new Index { Value = Encoding.Default.GetString(bytes) };

		public override int GetHashCode() => Value.GetHashCode();
	}
}
