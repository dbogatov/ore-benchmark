using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	public class TapeGen : CustomPRG
	{
		private readonly IPRG _generator;

		public TapeGen(byte[] key, byte[] entropy) : base(PRFFactory.GetPRF().PRF(key, entropy, key.Take(16).ToArray()))
		{
			_generator = PRGFactory.GetPRG(_seed);
		}

		public override void GetBytes(byte[] data)
		{
			_generator.NextBytes(data);
		}
	}
}
