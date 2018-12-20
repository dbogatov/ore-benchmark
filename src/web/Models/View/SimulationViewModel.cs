using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.View
{
	public class SimulationViewModel
	{
		public int? Seed { get; set; }
		
		[EnumDataType(typeof(ORESchemes.Shared.ORESchemes))]
		public ORESchemes.Shared.ORESchemes Protocol { get; set; } = ORESchemes.Shared.ORESchemes.NoEncryption;
		public string Dataset { get; set; }
		public string Queryset { get; set; }
		
		[Range(0, 100)]
		[Display(Name = "Cache size")]
		public int? CacheSize { get; set; }
	}
}
