using System.ComponentModel.DataAnnotations;

namespace Website.Models.Authorization
{
	public class LogoutModel
	{
		[Required]
		public bool SaveExchanges { get; set; }

		[Required]
		public string Button { get; set; }

		public string PreviousUrl { get; set; }
	}
}
