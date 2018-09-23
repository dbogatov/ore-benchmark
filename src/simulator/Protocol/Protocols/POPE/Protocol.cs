using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRG;

[assembly: InternalsVisibleTo("test")]

namespace Simulation.Protocol.POPE
{
	public class Protocol : AbsProtocol
	{
		public Protocol(byte[] entropy, int L = 60)
		{
			IPRG G = new PRGFactory(entropy).GetPrimitive();

			_client = new Client(G.GetBytes(128 / 8));
			_server = new Server(G.GetBytes(128 / 8), L);

			SetupProtocol();
		}
	}

	internal class Cipher : IGetSize
	{
		public byte[] encrypted;

		public int GetSize() => encrypted.Length * 8;
	}

	internal enum Origin
	{
		Left = 0b00,
		None = 0b01,
		Right = 0b11
	}

	internal class Value
	{
		private int value;
		private int nonce;
		private Origin origin;

		/// <summary>
		/// This number derived from value, nonce and origin should be used for comparisons
		/// </summary>
		public long OrderValue
		{
			get
			{
				return (long)value * (long)Int32.MaxValue + ((long)origin * (long)(Int32.MaxValue / 4)) + (long)nonce;
			}
		}

		public Value(int value, int nonce, Origin origin)
		{
			Debug.Assert(nonce < Int32.MaxValue / 4, $"Nonce must be from 0 to Int32.MaxValue / 4 = {Int32.MaxValue / 4}");
			Debug.Assert(nonce >= 0, $"Nonce must be from 0 to Int32.MaxValue / 4 = {Int32.MaxValue / 4}");
			
			this.value = value;
			this.nonce = nonce;
			this.origin = origin;
		}
	}
}
