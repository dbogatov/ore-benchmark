using System.ComponentModel.DataAnnotations;
using Simulation;

namespace Web.Models.View
{
	public enum CachePolicyView
	{
		[Display(Name = "Least-recently used (LRU)")]
		LRU,
		[Display(Name = "Least-frequently used (LFU)")]
		LFU,
		[Display(Name = "First-in, First-out (FIFO)")]
		FIFO
	}

	public enum ORESchemesView
	{
		[Display(Name = "No Encryption")]
		NoEncryption,
		[Display(Name = "BCLO OPE")]
		BCLO,
		[Display(Name = "CLWW ORE")]
		CLWW,
		[Display(Name = "Lewi-Wu ORE")]
		LewiWu,
		[Display(Name = "FH-OPE")]
		FHOPE,
		[Display(Name = "CLOZ ORE")]
		CLOZ,
		[Display(Name = "Kerschbaum Protocol")]
		Kerschbaum,
		[Display(Name = "POPE Protocol")]
		POPE,
		[Display(Name = "ORAM Protocol")]
		ORAM,
		[Display(Name = "Logarithmic-BRC SSE (CJJKRS'13) Protocol")]
		CJJKRS,
		[Display(Name = "Logarithmic-BRC SSE (CJJJKRS'14) Protocol")]
		CJJJKRS
	}

	public enum PrimitiveView
	{
		AES,
		[Display(Name = "PRF (function)")]
		PRF,
		[Display(Name = "Symmetric encryption")]
		Symmetric,
		[Display(Name = "PRG (generator)")]
		PRG,
		Hash,
		[Display(Name = "Length-flexible PRF")]
		LFPRF,
		[Display(Name = "PRP (permutation)")]
		PRP,
		[Display(Name = "Hyper-geometric sampler")]
		HGSampler,
		[Display(Name = "Uniform sampler")]
		UniformSampler,
		[Display(Name = "Binomial sampler")]
		BinomialSampler,
		[Display(Name = "Property-preserving hash")]
		PPH,
		[Display(Name = "Tree traversal (FH-OPE)")]
		TreeTraversal,
		[Display(Name = "ORAM path read / write")]
		ORAMPath,
		[Display(Name = "ORAM read / write request")]
		ORAMLevel,
		[Display(Name = "Tuple-set (SSE scheme)")]
		TSet
	}

	public class SimulationViewModel
	{
		public int? Seed { get; set; }

		[EnumDataType(typeof(ORESchemesView))]
		public ORESchemesView Protocol { get; set; } = ORESchemesView.NoEncryption;
		public Crypto.Shared.Protocols ProtocolReal
		{
			get
			{
				return (Crypto.Shared.Protocols)Protocol;
			}
		}

		[EnumDataType(typeof(CachePolicyView))]
		[Display(Name = "Cache policy")]
		public CachePolicyView CachePolicy { get; set; } = CachePolicyView.LFU;
		public CachePolicy CachePolicyReal
		{
			get
			{
				return (CachePolicy)CachePolicy;
			}
		}

		// Allow maximum of 10K lines of 16 characters
		[StringLength(16 * 10 * 1000, ErrorMessage = "Max dataset size is 160000 characters!")]
		public string Dataset { get; set; }

		[StringLength(16 * 10 * 1000, ErrorMessage = "Max queryset size is 160000 characters!")]
		public string Queryset { get; set; }

		[Range(0, 100)]
		[Display(Name = "Cache size")]
		public int? CacheSize { get; set; }

		[Range(2, 1024)]
		[Display(Name = "Elements per I/O page")]
		public int? ElementsPerPage { get; set; }
	}
}
