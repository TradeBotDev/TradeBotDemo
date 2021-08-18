using Google.Protobuf.Collections;
using TradeBot.Account.AccountService.v1;

namespace Website.Models
{
	public class AccountDataModel
	{
		public string Email { get; set; }

		public RepeatedField<ExchangeAccessInfo> Exchanges { get; set; }
	}
}
