using Serilog;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.LicenseMessages
{
    public static class SetLicenseReplies
    {
        public static SetLicenseResponse LicenseIsExists()
        {
            const string Message = "Произошла ошибка добавления лицензии: лицензия уже существует на данный продукт.";
            Log.Information(Message);

            return new SetLicenseResponse
            {
                Code = LicenseCode.IsExists,
                Message = Message
            };
        }

        public static SetLicenseResponse SuccessfulSettingLicense()
        {
            const string Message = "Успешное добавление лицензии.";
            Log.Information(Message);

            return new SetLicenseResponse
            {
                Code = LicenseCode.Successful,
                Message = Message
            };
        }
    }
}
