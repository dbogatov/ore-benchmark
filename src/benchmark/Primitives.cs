using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using BenchmarkDotNet.Attributes;

using ORESchemes.Shared.Primitives.Hash;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRP;
using ORESchemes.Shared.Primitives.Sampler;
using ORESchemes.Shared.Primitives.Symmetric;

namespace Benchmark.Primitives
{
	public class Benchmark
	{
		private IHash H;
		private IPRF F;
		private ISymmetric E;
		private IPRG G;
		private IPRG GDef;
		private IPRG GCached;
		private IPRP PFeistel;
		private IPRP PStrongFeistel;
		private ISimplifiedPRP PTable;
		private ISimplifiedPRP PNoInv;
		private ISampler S;
		private ISampler SDef;
		private ISampler SCached;

		private byte[] _key = new byte[128 / 8];

		public Benchmark()
		{
			H = new HashFactory().GetPrimitive();
			F = new PRFFactory().GetPrimitive();
			E = new SymmetricFactory().GetPrimitive();

			G = new PRGFactory().GetPrimitive();
			GDef = new DefaultPRGFactory().GetPrimitive();
			GCached = new PRGCachedFactory().GetPrimitive();

			PFeistel = new PRPFactory().GetPrimitive();
			PStrongFeistel = new StrongPRPFactory().GetPrimitive();

			PTable = new TablePRPFactory().GetPrimitive();
			PNoInv = new NoInvPRPFactory().GetPrimitive();

			S = new SamplerFactory(G).GetPrimitive();
			SDef = new SamplerFactory(GDef).GetPrimitive();
			SCached = new SamplerFactory(GCached).GetPrimitive();

			var random = new Random(123456);
			random.NextBytes(_key);
		}

		[Benchmark]
		[ArgumentsSource(nameof(BytesData))]
		public void Hash(byte[] input, int size) => H.ComputeHash(input);


		[Benchmark(Baseline = true)]
		[ArgumentsSource(nameof(BytesData))]
		public void PRF(byte[] input, int size) => F.PRF(_key, input);

		[Benchmark]
		[ArgumentsSource(nameof(PRFBytesData))]
		public void InversePRF(byte[] input, int size) => F.InversePRF(_key, input);

		[Benchmark]
		[ArgumentsSource(nameof(BytesData))]
		public void SymmetricEnc(byte[] input, int size) => E.Encrypt(_key, input);

		[Benchmark]
		[ArgumentsSource(nameof(SymmetricBytesData))]
		public void SymmetricDec(byte[] input, int size) => E.Decrypt(_key, input);


		[Benchmark]
		public void PRGNext() => G.Next();

		[Benchmark]
		[ArgumentsSource(nameof(BytesData))]
		public void PRGBytes(byte[] output, int size) => G.NextBytes(output);

		[Benchmark]
		public void PRGDefaultNext() => GDef.Next();

		[Benchmark]
		[ArgumentsSource(nameof(BytesData))]
		public void PRGDefaultBytes(byte[] output, int size) => GDef.NextBytes(output);

		[Benchmark]
		public void PRGCachedNext() => GCached.Next();

		[Benchmark]
		[ArgumentsSource(nameof(BytesData))]
		public void PRGCachedBytes(byte[] output, int size) => GCached.NextBytes(output);


		[Benchmark]
		[ArgumentsSource(nameof(BitData))]
		public void PRP(BitArray input, int size) => PFeistel.PRP(input, _key);

		[Benchmark]
		[ArgumentsSource(nameof(BitData))]
		public void InversePRP(BitArray input, int size) => PFeistel.InversePRP(input, _key);

		[Benchmark]
		[ArgumentsSource(nameof(BitData))]
		public void PRPStrong(BitArray input, int size) => PStrongFeistel.PRP(input, _key);

		[Benchmark]
		[ArgumentsSource(nameof(BitData))]
		public void InversePRPStrong(BitArray input, int size) => PStrongFeistel.InversePRP(input, _key);

		[Benchmark]
		[ArgumentsSource(nameof(ByteData))]
		public void PRPTable(byte input, int size) => PTable.PRP(input, _key, (byte)size);

		[Benchmark]
		[ArgumentsSource(nameof(ByteData))]
		public void InversePRPTable(byte input, int size) => PTable.InversePRP(input, _key, (byte)size);

		[Benchmark]
		[ArgumentsSource(nameof(ByteData))]
		public void PRPNoInv(byte input, int size) => PNoInv.PRP(input, _key, (byte)size);

		[Benchmark]
		[ArgumentsSource(nameof(ByteData))]
		public void InversePRPNoInv(byte input, int size) => PNoInv.InversePRP(input, _key, (byte)size);


		[Benchmark]
		[Arguments(0, 1)]
		[Arguments(5, 55)]
		[Arguments(5, 555)]
		[Arguments(5, 55555)]
		[Arguments(0, ulong.MaxValue)]
		public void SamplerUniform(ulong from, ulong to) => S.Uniform(from, to);

		[Benchmark]
		[Arguments(99, 10, 25)]
		[Arguments(500, 50, 100)]
		[Arguments(500, 60, 200)]
		[Arguments(500, 70, 300)]
		[Arguments(UInt16.MaxValue, UInt16.MaxValue / 4, UInt16.MaxValue / 2)]
		[Arguments(UInt32.MaxValue, UInt32.MaxValue / 4, UInt32.MaxValue / 2)]
		[Arguments(UInt64.MaxValue / 100, UInt32.MaxValue, unchecked(2 * UInt32.MaxValue))]
		public void SamplerHG(ulong population, ulong success, ulong samples) => S.HyperGeometric(population, success, samples);

		[Benchmark]
		[Arguments(0, 1)]
		[Arguments(5, 55)]
		[Arguments(5, 555)]
		[Arguments(5, 55555)]
		[Arguments(0, ulong.MaxValue)]
		public void SamplerDefaultUniform(ulong from, ulong to) => SDef.Uniform(from, to);

		[Benchmark]
		[Arguments(99, 10, 25)]
		[Arguments(500, 50, 100)]
		[Arguments(500, 60, 200)]
		[Arguments(500, 70, 300)]
		[Arguments(UInt16.MaxValue, UInt16.MaxValue / 4, UInt16.MaxValue / 2)]
		[Arguments(UInt32.MaxValue, UInt32.MaxValue / 4, UInt32.MaxValue / 2)]
		[Arguments(UInt64.MaxValue / 100, UInt32.MaxValue, unchecked(2 * UInt32.MaxValue))]
		public void SamplerDefaultHG(ulong population, ulong success, ulong samples) => SDef.HyperGeometric(population, success, samples);

		[Benchmark]
		[Arguments(0, 1)]
		[Arguments(5, 55)]
		[Arguments(5, 555)]
		[Arguments(5, 55555)]
		[Arguments(0, ulong.MaxValue)]
		public void SamplerCachedUniform(ulong from, ulong to) => SCached.Uniform(from, to);

		[Benchmark]
		[Arguments(99, 10, 25)]
		[Arguments(500, 50, 100)]
		[Arguments(500, 60, 200)]
		[Arguments(500, 70, 300)]
		[Arguments(UInt16.MaxValue, UInt16.MaxValue / 4, UInt16.MaxValue / 2)]
		[Arguments(UInt32.MaxValue, UInt32.MaxValue / 4, UInt32.MaxValue / 2)]
		[Arguments(UInt64.MaxValue / 100, UInt32.MaxValue, unchecked(2 * UInt32.MaxValue))]
		public void SamplerCachedHG(ulong population, ulong success, ulong samples) => SCached.HyperGeometric(population, success, samples);


		public IEnumerable<object[]> BytesData()
		{
			var random = new Random(123456);

			foreach (var size in new[] { 64, 128, 256, 1024, 4096 })
			{
				byte[] bytes = new byte[size / 8];
				random.NextBytes(bytes);
				yield return new object[] { bytes, size };
			}
		}

		public IEnumerable<object[]> ByteData()
		{
			var random = new Random(123456);

			foreach (var size in new[] { 1, 2, 4, 8 })
			{
				yield return new object[] { random.Next(0, (int)(Math.Pow(2, size) - 1)), size };
			}
		}

		public IEnumerable<object[]> SmallBitData()
		{
			var random = new Random(123456);

			foreach (var size in new[] { 1, 2, 4, 8 })
			{
				yield return new object[] { new BitArray(new int[] { random.Next(0, (int)(Math.Pow(2, size) - 1)) }), size };
			}
		}

		public IEnumerable<object[]> BitData() => BytesData().Select(b => new object[] { new BitArray((byte[])b[0]), b[1] }).Concat(SmallBitData());

		public IEnumerable<object[]> PRFBytesData() => BytesData().Select(b => new object[] { F.PRF(_key, (byte[])b[0]), b[1] });

		public IEnumerable<object[]> SymmetricBytesData() => BytesData().Select(b => new object[] { E.Encrypt(_key, (byte[])b[0]), b[1] });
	}
}
