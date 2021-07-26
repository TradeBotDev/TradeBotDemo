using Serilog;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class CurrentAccountReplies
    {
        public static CurrentAccountReply AccountNotFound()
        {
            const string Message = "Ошибка при получении данных текущего аккаунта: пользователь не найден.";
            Log.Information(Message);

            return new CurrentAccountReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Message,
                CurrentAccount = null
            };
        }

        public static CurrentAccountReply SuccessfulGettingAccountData(AccountInfo currentAccount)
        {
            const string Message = "Получение данных текущего пользователя.";
            Log.Information(Message);

            return new CurrentAccountReply
            {
                Result = ActionCode.Successful,
                Message = Message,
                CurrentAccount = currentAccount
            };
        }
    }
}
