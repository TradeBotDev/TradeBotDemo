using Serilog;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.LicenseMessages
{
    public static class GetKeyReplies
    {
        public static GetKeyResponse LicenseIsExists(string key)
        {
            const string Message = "Ключ лицензии получен успешно.";
            Log.Information(Message);

            return new GetKeyResponse
            {
                Code = LicenseCode.HaveAccess,
                Message = Message,
                Key = key
            };
        }

        public static GetKeyResponse LicenseIsNotExists()
        {
            const string Message = "Произошла ошибка: данный ключ не существует.";
            Log.Information(Message);

            return new GetKeyResponse
            {
                Code = LicenseCode.NoAccess,
                Message = Message,
                Key = null
            };
        }
    }
}
