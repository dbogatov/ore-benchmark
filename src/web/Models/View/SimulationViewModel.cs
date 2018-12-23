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
		POPE
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
		public string Dataset { get; set; }
		public string Queryset { get; set; }

		[Range(0, 100)]
		[Display(Name = "Cache size")]
		public int? CacheSize { get; set; }
	}
}
