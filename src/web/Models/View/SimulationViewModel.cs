using System.ComponentModel.DataAnnotations;

namespace Web.Models.View
{
	public enum ORESchemesView
	{
		[Display(Name = "No Encryption")]
		NoEncryption,
		[Display(Name = "BCLO OPE")]
		CryptDB,
		[Display(Name = "CLWW ORE")]
		PracticalORE,
		[Display(Name = "Lewi-Wu ORE")]
		LewiORE,
		[Display(Name = "FH-OPE")]
		FHOPE,
		[Display(Name = "CLOZ ORE")]
		AdamORE,
		[Display(Name = "Kerschbaum Protocol")]
		Florian,
		[Display(Name = "POPE Protocol")]
		POPE,
		[Display(Name = "ORAM Protocol")]
		ORAM,
		[Display(Name = "Logarithmic-BRC SSE Protocol")]
		SSE
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
		public ORESchemes.Shared.ORESchemes ProtocolReal
		{
			get
			{
				return (ORESchemes.Shared.ORESchemes)Protocol;
			}
		}
		
		// Allow maximum of 10K lines of 64 characters
		[StringLength(64*10*1000, ErrorMessage = "Max dataset size is 640000 characters!")]
		public string Dataset { get; set; }
		
		[StringLength(64*10*1000, ErrorMessage = "Max queryset size is 640000 characters!")]
		public string Queryset { get; set; }

		[Range(0, 100)]
		[Display(Name = "Cache size")]
		public int? CacheSize { get; set; }
		
		[Range(2, 1024)]
		[Display(Name = "Elements per I/O page")]
		public int? ElementsPerPage { get; set; }
	}
}
