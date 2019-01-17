# ORE / OPE / SSE schemes and related crypto primitives

These are the open-source implementation of the following OPE / ORE schemes

- [Order Preserving Symmetric Encryption (aka CryptDB OPE)](https://eprint.iacr.org/2012/624.pdf)
- [Practical Order-Revealing Encryption with Limited Leakage (aka Practical ORE)](https://eprint.iacr.org/2015/1125.pdf)
- [Order-Revealing Encryption: New Constructions, Applications, and Lower Bounds (aka Lewi ORE)](https://eprint.iacr.org/2016/612.pdf)
- [Frequency-Hiding Order-Preserving Encryption (aka FH-OPE)](http://www.fkerschbaum.org/ccs15.pdf)
- [CJJKRS '13 SSE scheme (aka CJJKRS)](https://eprint.iacr.org/2013/169.pdf)
- [CJJJKRS '14 SSE scheme (aka CJJJKRS)](https://eprint.iacr.org/2014/853.pdf)

The primitives include

- PRG (AES-CTR based, caching extra entropy)
- PRF (AES-ECB based, no IV)
- PRP (Feistel networks 3/4 rounds, Knuth shuffle, caching permutation)
- Hash (wrappers for SHA256, SHA512, optionally include key)
- LF-PRF (from BCLO paper)
- Samplers (hyper-geometric from BCLO, binomial, uniform)
- Symmetric encryption (AES-CBC based)
- T-set (from CJJKRS paper)

> This implementation is for research purposes only.
> It is not advised to use in enterprise solutions.

This implementation is exported as a NuGet packages

- [Primitives](https://www.nuget.org/packages/ore-benchamrk.shared/)
- [CJJJKRS](https://www.nuget.org/packages/cjjjkrs-sse/)
- [CryptDB OPE](https://www.nuget.org/packages/bclo-ope/)
- [Practical ORE](https://www.nuget.org/packages/clww-ore/)
- [Lewi ORE](https://www.nuget.org/packages/lewi-wu-ore/)
- [FH-OPE](https://www.nuget.org/packages/fh-ope/)
- [CJJKRS](https://www.nuget.org/packages/cjjkrs-sse/)
- [CJJJKRS](https://www.nuget.org/packages/cjjjkrs-sse/)

Primitive documentation is hosted [here](https://ore.dbogatov.org/documentation/).

Here is how to add the dependencies (in `.csproj` file)

	<PackageReference Include="ore-benchamrk.shared" Version="*" />
	<PackageReference Include="clww-ore" Version="*" />
	<PackageReference Include="lewi-wu-ore" Version="*" />
	<PackageReference Include="fh-ope" Version="*" />
	<PackageReference Include="bclo-ope" Version="*" />
	<PackageReference Include="cjjkrs-sse" Version="*" />
	<PackageReference Include="cjjjkrs-sse" Version="*" />

## Code examples

### OPE / ORE Schemes

```cs
// Choose one of the schemes

ORESchemes.CryptDBOPE.CryptDBScheme scheme =
	new ORESchemes.CryptDBOPE.CryptDBScheme(
		int.MinValue,
		int.MaxValue,
		Convert.ToInt64(-Math.Pow(2, 48)),
		Convert.ToInt64(Math.Pow(2, 48))
	);

ORESchemes.PracticalORE.PracticalOREScheme scheme =
	new ORESchemes.PracticalORE.PracticalOREScheme();

ORESchemes.LewiORE.LewiOREScheme scheme =
	new ORESchemes.LewiORE.LewiOREScheme();

ORESchemes.FHOPE.FHOPEScheme scheme =
	new ORESchemes.FHOPE.FHOPEScheme(long.MinValue, long.MaxValue);

// Scheme operations

var key = scheme.KeyGen();

var cipher5 = scheme.Encrypt(5, key);
var cipher6 = scheme.Encrypt(6, key);

// If FH-OPE then we need max and min for ciphers to compare 
cipher5.max = scheme.MaxCiphertext(5, key);
cipher5.min = scheme.MinCiphertext(5, key);
cipher6.max = scheme.MaxCiphertext(6, key);
cipher6.min = scheme.MinCiphertext(6, key);

scheme.IsLess(cipher5, cipher6); // true
scheme.IsLessOrEqual(cipher5, cipher6); // true
scheme.IsEqual(cipher5, cipher6); // false
scheme.IsGreaterOrEqual(cipher5, cipher6); // false
scheme.IsGreater(cipher5, cipher6); // false

int plaint5 = scheme.Decrypt(cipher5, key); // 5
int plaint6 = scheme.Decrypt(cipher6, key); // 6
```

### SSE

### Primitives

<!-- TODO -->
