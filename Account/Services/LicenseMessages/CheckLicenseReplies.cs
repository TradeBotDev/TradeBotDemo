using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.LicenseMessages
{
    public static class CheckLicenseReplies
    {
        public static CheckLicenseResponse LicenseIsExists()
        {
            const string Message = "Данный пользователь обладает лицензией на продукт.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Code = LicenseCode.HaveAccess,
                Message = Message,
                HaveAccess = true
            };
        }

        public static CheckLicenseResponse LicenseIsNotExists()
        {
            const string Message = "Произошла ошибка проверки лицензии: данный пользователь не обладает лицензией на продукт.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message,
                HaveAccess = false
            };
        }

        public static CheckLicenseResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка проверки лицензии: пользователь не найден.";
            Log.Information(Message);

            return new CheckLicenseResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message,
                HaveAccess = false
            };
        }
    }
}
