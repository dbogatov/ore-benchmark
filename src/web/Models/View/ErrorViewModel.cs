namespace Web.Models.View
{
	public class ErrorViewModel
	{
		public int Code { get; set; }

		public string Message
		{
			get
			{
				switch (Code)
				{
					case 404:
						return @"
							The page not found. 
							Most likely, your simulation has been cleaned up.
						";
					default:
						return @"
							Generic error occurred. 
							We are investigating.
						";
				}
			}
		}
	}
}
