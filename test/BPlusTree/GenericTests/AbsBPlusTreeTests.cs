using BPlusTree;
using ORESchemes.Shared;

namespace Test.BPlusTree
{
	public abstract partial class AbsBPlusTree<C, K>
		where C : IGetSize
		where K : IGetSize
	{
		protected readonly IOREScheme<C, K> _scheme;
		protected readonly K _key;

		protected readonly int _max;

		protected Options<C> _defaultOptions
		{
			get
			{
				var options = new Options<C>(_scheme, 3);
				options.MinCipher = _scheme.MinCiphertextValue(_key);
				options.MaxCipher = _scheme.MaxCiphertextValue(_key);
				return options;
			}
			private set { }
		}

		public AbsBPlusTree(IOREScheme<C, K> scheme, int max = 1000)
		{
			_max = max;

			_scheme = scheme;

			_scheme.Init();
			_key = scheme.KeyGen();
		}

		private Options<C> OptionsWithBranching(int branching)
		{
			var options = new Options<C>(_scheme, branching);
			options.MinCipher = _scheme.MinCiphertextValue(_key);
			options.MaxCipher = _scheme.MaxCiphertextValue(_key);
			return options;
		}
	}
}
