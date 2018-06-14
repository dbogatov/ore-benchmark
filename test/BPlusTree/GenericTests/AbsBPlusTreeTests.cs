using System;
using Xunit;
using DataStructures.BPlusTree;
using ORESchemes.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTreeTests<C, K>
	{
		protected readonly IOREScheme<C, K> _scheme;
		protected readonly K _key;

		protected readonly int _max;

		protected Options<C> _defaultOptions
		{
			get
			{
				return new Options<C>(
					_scheme,
					_scheme.MinCiphertextValue(_key),
					_scheme.MaxCiphertextValue(_key),
					3
				);
			}
			private set { }
		}

		public AbsBPlusTreeTests(IOREScheme<C, K> scheme, int max = 1000)
		{
			_max = max;

			_scheme = scheme;

			_scheme.Init();
			_key = scheme.KeyGen();
		}
	}
}
