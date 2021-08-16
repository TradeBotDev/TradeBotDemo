using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class AccountDataReplies
    {
        public static AccountDataResponse AccountNotFound()
        {
            const string Message = "Ошибка при получении данных текущего аккаунта: пользователь не найден.";
            Log.Information(Message);

            return new AccountDataResponse
            {
                Result = AccountActionCode.IsNotFound,
                Message = Message,
                CurrentAccount = null
            };
        }

        public static AccountDataResponse TimePassed()
        {
            const string Message = "Ошибка при получении данных текущего аккаунта: время сессии вышло.";
            Log.Information(Message);

            return new AccountDataResponse
            {
                Result = AccountActionCode.TimePassed,
                Message = Message,
                CurrentAccount = null
            };
        }

        public static AccountDataResponse SuccessfulGettingAccountData(Models.LoggedAccount currentAccount)
        {
            const string Message = "Получение данных текущего пользователя завершено успешно.";
            Log.Information(Message);

            var reply = new AccountDataResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message,
                CurrentAccount = new AccountInfo
                {
                    AccountId = currentAccount.AccountId,
                    Email = currentAccount.Account.Email
                }
            };
            foreach (Models.ExchangeAccess exchange in currentAccount.Account.ExchangeAccesses)
            {
                reply.CurrentAccount.Exchanges.Add(new ExchangeAccessInfo
                {
                    ExchangeAccessId = exchange.ExchangeAccessId,
                    Code = exchange.Code,
                    Name = exchange.Name,
                    Token = exchange.Token,
                    Secret = exchange.Secret
                });
            }
            return reply;
        }
    }
}
