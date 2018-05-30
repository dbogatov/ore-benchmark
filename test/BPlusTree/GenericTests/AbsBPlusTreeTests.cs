using System;
using Xunit;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C>
	{
		protected readonly IOREScheme<C> _scheme;
		protected readonly byte[] _key;

		protected readonly int _max;

		public AbsBPlusTreeTests(IOREScheme<C> scheme, int max = 1000)
		{
			_max = max;

			_scheme = scheme;

			_scheme.Init();
			_key = scheme.KeyGen();
		}
	}
}
