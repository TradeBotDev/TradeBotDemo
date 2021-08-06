using Serilog;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.LicenseMessages
{
    public static class CheckLicenseReplies
    {
        public static CheckLicenseResponse LicenseIsExists()
        {
            const string Message = "Данная лицензия существует..";
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
            const string Message = "Произошла ошибка: данная лицензия не существует.";
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
