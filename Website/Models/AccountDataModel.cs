using Google.Protobuf.Collections;
using TradeBot.Facade.FacadeService.v1;

namespace Website.Models
{
	public class AccountDataModel
	{
		public string Email { get; set; }

		public RepeatedField<ExchangeAccessInfo> Exchanges { get; set; }
	}
}
