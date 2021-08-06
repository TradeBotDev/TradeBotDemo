using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class IsLicensedReplies
    {
        public static IsLicensedResponse HaveAccess()
        {
            const string Message = "Лицензия доступна.";
            Log.Information(Message);

            return new IsLicensedResponse
            {
                Access = true,
                Message = Message
            };
        }

        public static IsLicensedResponse NotHaveAccess()
        {
            const string Message = "Произошла ошибка при проверке лицензии: доступ к лицензии отсутствует.";
            Log.Information(Message);

            return new IsLicensedResponse
            {
                Access = false,
                Message = Message
            };
        }

        public static IsLicensedResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка при проверке лицензии: пользователь не найден.";
            Log.Information(Message);

            return new IsLicensedResponse
            {
                Access = false,
                Message = Message
            };
        }
    }
}
