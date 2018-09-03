# B+ tree

This is an open-source implementation of B+ tree as defined in [the original paper](http://www.inf.fu-berlin.de/lehre/SS10/DBS-Intro/Reader/BayerBTree-72.pdf) with the proper deletion algorithm as defined in [Jan Jannink's paper](https://dl.acm.org/citation.cfm?id=202666).

> This implementation is for research purposes only.
> It is not advised to use in enterprise solutions.

This implementation is exported as a [NuGet package](https://www.nuget.org/packages/b-plus-tree/).
Primitive documentation is hosted [here](https://ore.dbogatov.org/api/BPlusTree.ITree-2.htm).

Here is how to add a dependency (in `.csproj` file)

	<PackageReference Include="b-plus-tree" Version="*" />

Here is the usage example

```cs
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
```
