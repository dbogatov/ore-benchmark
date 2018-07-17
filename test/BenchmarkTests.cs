using System;
using System.Collections;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.Symmetric;
using Xunit;

namespace Test
{
	[Trait("Category", "Unit")]
	public class Benchmark
	{
		[Theory]
		[InlineData("Primitives")]
		[InlineData("Schemes")]
		public void DryJob(string @namespace)
		{
			var summary = BenchmarkSwitcher.FromTypes(
				new[] {
					typeof(global::Benchmark.Schemes.Benchmark<OPECipher, BytesKey>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.PracticalORE.Ciphertext, BytesKey>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.LewiORE.Ciphertext, global::ORESchemes.LewiORE.Key>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.FHOPE.Ciphertext, global::ORESchemes.FHOPE.State>),
					typeof(global::Benchmark.Primitives.Benchmark)
				}
			).Run(new[] { $"--namespace=Benchmark.{@namespace}", "--join" }, new CustomConfig());
		}

		private class CustomConfig : ManualConfig
		{
			public CustomConfig() => Add(Job.Dry.With(InProcessToolchain.Instance));
		}

		[Fact]
		public void ManualRun()
		{
			Random G = new Random(123465);

			byte[] key = new byte[128 / 8];
			G.NextBytes(key);

			byte[] bytes = new byte[1024 / 8];
			G.NextBytes(bytes);

			byte[] bytesInv = new PRFFactory().GetPrimitive().PRF(key, bytes);
			byte[] bytesEnc = new SymmetricFactory().GetPrimitive().Encrypt(key, bytes);

			BitArray bits = new BitArray(new int[] { G.Next(byte.MinValue, byte.MaxValue) });
			byte @byte = (byte)G.Next(byte.MinValue, byte.MaxValue);

			ulong from = (ulong)G.Next(0, int.MaxValue / 2);
			ulong to = (ulong)G.Next(int.MaxValue / 2 + 1, int.MaxValue);

			ulong population = 500;
			ulong success = 70;
			ulong samples = 300;

			var benchmark = new global::Benchmark.Primitives.Benchmark();
			benchmark.key = key;

			benchmark.Hash(bytes, 1024);

			benchmark.PRF(bytes, 1024);
			benchmark.InversePRF(bytesInv, 1024);

			benchmark.SymmetricEnc(bytes, 1024);
			benchmark.SymmetricDec(bytesEnc, 1024);

			benchmark.PRGNext();
			benchmark.PRGBytes(bytes, 1024);
			benchmark.PRGDefaultNext();
			benchmark.PRGDefaultBytes(bytes, 1024);
			benchmark.PRGCachedNext();
			benchmark.PRGCachedBytes(bytes, 1024);

			benchmark.PRP(bits, 8);
			benchmark.InversePRP(bits, 8);
			benchmark.PRPStrong(bits, 8);
			benchmark.InversePRPStrong(bits, 8);
			benchmark.PRPTable(@byte, 8);
			benchmark.InversePRPTable(@byte, 8);
			benchmark.PRPNoInv(@byte, 8);
			benchmark.InversePRPNoInv(@byte, 8);

			benchmark.SamplerUniform(from, to);
			benchmark.SamplerHG(population, success, samples);
			benchmark.SamplerDefaultUniform(from, to);
			benchmark.SamplerDefaultHG(population, success, samples);
			benchmark.SamplerCachedUniform(from, to);
			benchmark.SamplerCachedHG(population, success, samples);
		}
	}
}
