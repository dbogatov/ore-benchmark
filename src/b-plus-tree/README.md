# B+ tree

This is an open-source implementation of B+ tree as defined in [the original paper](http://www.inf.fu-berlin.de/lehre/SS10/DBS-Intro/Reader/BayerBTree-72.pdf) with the proper deletion algorithm as defined in [Jan Jannink's paper](https://dl.acm.org/citation.cfm?id=202666).

> This implementation is for research purposes only.
> It is not advised to use in enterprise solutions.

This implementation is exported as a [NuGet package](https://www.nuget.org/packages/b-plus-tree/).
Primitive documentation is hosted [here](https://ore.dbogatov.org/documentation/api/BPlusTree.ITree-2.html). 
<!-- TODO -->

Here is how to add a dependency (in `.csproj` file)

	<PackageReference Include="b-plus-tree" Version="*" />

## Code examples

See code examples [here](https://github.com/dbogatov/ore-benchmark/tree/master/tools/packages-example).
