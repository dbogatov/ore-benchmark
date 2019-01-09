using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ORESchemes.CJJKRS;
using ORESchemes.Shared;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.SSE
{
	public class Word : IWord
	{
		public (BitArray, int) Value { get; set; }

		/// <summary>
		/// BitArray will be given by Cover.BRC method, so all BitArray values
		/// will be of equal length (32 bits). Therefore, this method will output
		/// unique bytes representation of the tuple.
		/// </summary>
		public byte[] ToBytes()
			=> Value.Item1.ToBytes().Concat(BitConverter.GetBytes(Value.Item2)).ToArray();
	}

	public class Index : IIndex
	{
		public string Value { get; set; }

		public byte[] ToBytes()
			=> Encoding.Default.GetBytes(Value);
			
		public static Index FromBytes(byte[] bytes)
			=> new Index { Value = Encoding.Default.GetString(bytes) };
	}

	public class Protocol : AbsProtocol
	{
		public Protocol(byte[] entropy, int elementsPerPage)
		{
			_client = new Client(entropy);
			_server = new Server(elementsPerPage);

			SetupProtocol();
		}
	}
}
