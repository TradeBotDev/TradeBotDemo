using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class CheckLicenseReplies
    {
        public static CheckLicenseResponse HaveAccess()
        {
            const string Message = "Лицензия доступна.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Access = true,
                Message = Message
            };
        }

        public static CheckLicenseResponse NotHaveAccess()
        {
            const string Message = "Произошла ошибка при проверке лицензии: доступ к лицензии отсутствует.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Access = false,
                Message = Message
            };
        }

        public static CheckLicenseResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка при проверке лицензии: пользователь не найден.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Access = false,
                Message = Message
            };
        }
    }
}
