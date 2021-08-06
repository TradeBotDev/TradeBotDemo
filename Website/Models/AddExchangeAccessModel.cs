using System.ComponentModel.DataAnnotations;
using TradeBot.Account.AccountService.v1;

namespace Website.Models
{
    public class AddExchangeAccessModel
    {
        [Required]
        public ExchangeAccessCode ExchangeCode { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Secret { get; set; }
    }
}
