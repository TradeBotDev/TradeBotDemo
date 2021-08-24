using System.ComponentModel.DataAnnotations;
using TradeBot.Facade.FacadeService.v1;

namespace Website.Models
{
	public class AddExchangeAccessModel
	{
		[Required(ErrorMessage = "Биржа не выбрана.")]
		public ExchangeAccessCode ExchangeCode { get; set; }

		[Required(ErrorMessage = "Токен отсутствует.")]
		public string Token { get; set; }

		[Required(ErrorMessage = "Секрет отсутствует.")]
		public string Secret { get; set; }
	}
}
