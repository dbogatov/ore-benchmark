using System.Collections.Generic;
using System.Linq;
using BPlusTree;
using ORESchemes.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Unit")]
	public class PublicBPlusTree : AbsBPlusTree<OPECipher, BytesKey>
	{
		public PublicBPlusTree() : base(new NoEncryptionScheme(new byte[] { 13, 05, 19, 96 })) { }

		protected override ITree<string, OPECipher> GetTree(Options<OPECipher> options) =>
			(ITree<string, OPECipher>)new global::BPlusTree.BPlusTree<string>(options.Branching);

		[Fact]
		/// <summary>
		/// Arose from bug
		/// </summary>
		public void BasicOperation()
		{
			var tree = new global::BPlusTree.BPlusTree<string>(60);

			tree.Insert(5, "five");
			tree.Insert(7, "seven");
			tree.Insert(9, "nine");

			string value;
			tree.TryGetSingle(5, out value);
			Assert.Equal("five", value);

			tree.UpdateSingle(5, "5");

			List<string> rangeResult = new List<string>();
			tree.TryRange(4, 8, rangeResult);
			Assert.Equal(
				new List<string> { "5", "seven" }.OrderBy(p => p),
				rangeResult.OrderBy(p => p)
			);

			tree.Delete(7);
			Assert.False(tree.TryGet(7, null));

			// Methods also accept optional predicate for duplicates
			tree.Insert(9, "another-nine");
			tree.Insert(9, "nine-again");

			rangeResult.Clear();
			tree.TryGet(9, rangeResult, val => val.StartsWith("nine"));
			Assert.Equal(
				new List<string> { "nine", "nine-again" }.OrderBy(p => p),
				rangeResult.OrderBy(p => p)
			);
		}
	}
}
