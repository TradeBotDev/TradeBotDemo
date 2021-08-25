using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.LicenseMessages
{
    public static class SetLicenseReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static SetLicenseResponse LicenseIsExists()
        {
            const string Message = "Произошла ошибка добавления лицензии: лицензия уже существует на данный продукт.";
            logger.Information("{@Replies} - " + Message, nameof(SetLicenseReplies));

            return new SetLicenseResponse
            {
                Code = LicenseCode.IsExists,
                Message = Message
            };
        }

        public static SetLicenseResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка добавления лицензии: пользователь не найден.";
            logger.Information("{@Replies} - " + Message, nameof(SetLicenseReplies));

            return new SetLicenseResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message
            };
        }

        public static SetLicenseResponse SuccessfulSettingLicense()
        {
            const string Message = "Успешное добавление лицензии.";
            logger.Information("{@Replies} - " + Message, nameof(SetLicenseReplies));

            return new SetLicenseResponse
            {
                Code = LicenseCode.Successful,
                Message = Message
            };
        }
    }
}
