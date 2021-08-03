using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.LicenseMessages
{
    public static class SetLicenseReplies
    {
        public static SetLicenseResponse LicenseIsExists()
        {
            const string Message = "Произошла ошибка добавления лицензии: лицензия уже существует на данный продукт.";

            return new SetLicenseResponse
            {
                Code = LicenseCode.IsExists,
                Message = Message
            };
        }

        public static SetLicenseResponse SuccessfulSettingLicense()
        {
            const string Message = "Успешное добавление лицензии.";

            return new SetLicenseResponse
            {
                Code = LicenseCode.Successful,
                Message = Message
            };
        }
    }
}
