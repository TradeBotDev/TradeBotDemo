using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class CurrentAccountReplies
    {
        public static AccountDataReply AccountNotFound()
        {
            const string Message = "Ошибка при получении данных текущего аккаунта: пользователь не найден.";
            Log.Information(Message);

            return new AccountDataReply
            {
                Result = AccountActionCode.IsNotFound,
                Message = Message,
                CurrentAccount = null
            };
        }

        public static AccountDataReply SuccessfulGettingAccountData(AccountInfo currentAccount)
        {
            const string Message = "Получение данных текущего пользователя.";
            Log.Information(Message);

            return new AccountDataReply
            {
                Result = AccountActionCode.Successful,
                Message = Message,
                CurrentAccount = currentAccount
            };
        }
    }
}
