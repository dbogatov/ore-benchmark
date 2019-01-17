using System.Collections.Generic;

namespace Packages
{
	partial class Program
	{
		static private void BPlusTree()
		{
			var tree = new BPlusTree.BPlusTree<string>(60);

			tree.Insert(5, "five");
			tree.Insert(7, "seven");
			tree.Insert(9, "nine");

			string value;
			tree.TryGetSingle(5, out value);
			// value is now "five"

			tree.UpdateSingle(5, "5");

			List<string> rangeResult = new List<string>();
			tree.TryRange(4, 8, rangeResult);
			// rangeResult is now ["5", "seven"]

			tree.Delete(7);

			// Methods also accept optional predicate for duplicates
			tree.Insert(9, "another-nine");
			tree.Insert(9, "nine-again");

			rangeResult.Clear();
			tree.TryGet(9, rangeResult, val => val.StartsWith("nine"));
			// rangeResult is now ["nine", "nine-again"]
		}
	}
}
