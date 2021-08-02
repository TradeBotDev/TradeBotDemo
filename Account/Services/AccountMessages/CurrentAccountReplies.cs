using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class CurrentAccountReplies
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

        public static AccountDataResponse SuccessfulGettingAccountData(AccountInfo currentAccount)
        {
            const string Message = "Получение данных текущего пользователя.";
            Log.Information(Message);

            return new AccountDataResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message,
                CurrentAccount = currentAccount
            };
        }
    }
}
