using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.LicenseMessages
{
    public static class CheckLicenseReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static CheckLicenseResponse LicenseIsExists()
        {
            const string Message = "Данный пользователь обладает лицензией на продукт.";
            logger.Information("{@Replies} - " + Message, nameof(CheckLicenseReplies));

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
            logger.Information("{@Replies} - " + Message, nameof(CheckLicenseReplies));

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
            logger.Information("{@Replies} - " + Message, nameof(CheckLicenseReplies));

            return new CheckLicenseResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message,
                HaveAccess = false
            };
        }
    }
}
