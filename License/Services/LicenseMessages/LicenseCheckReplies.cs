using Serilog;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.LicenseMessages
{
    public static class LicenseCheckReplies
    {
        public static LicenseCheckResponse LicenseIsExists()
        {
            const string Message = "Данная лицензия существует..";
            Log.Information(Message);

            return new LicenseCheckResponse
            {
                Code = LicenseCode.HaveAccess,
                Message = Message,
                HaveAccess = true
            };
        }

        public static LicenseCheckResponse LicenseIsNotExists()
        {
            const string Message = "Произошла ошибка: данная лицензия не существует.";
            Log.Information(Message);

            return new LicenseCheckResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message,
                HaveAccess = false
            };
        }
    }
}
