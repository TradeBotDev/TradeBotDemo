using System;
using System.Linq;
using System.Threading.Tasks;

using Grpc.Core;
using Serilog;

using LicenseGRPC.LicenseMessages;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC
{
    public class LicenseService : License.LicenseBase
    {
        // Метод, позволяющий установить лицензию для определенного пользователя на определенный продукт.
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            Log.Information($"SetLicense получил запрос: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                // Поиск существующей лицензии на данный продукт.
                bool licenseIsExists = database.Licenses.Any(license => license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                // В случае, если такой был найден, возвращается сообщение о том, что лицензия уже есть.
                if (licenseIsExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());
                else
                {
                    // Иначе создается новая лицензия и записывается в базу данных.
                    var license = new Models.License
                    {
                        AccountId = request.AccountId,
                        Product = request.Product
                    };

                    database.Licenses.Add(license);
                    database.SaveChanges();
                    return Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
                }
            }
        }

        // Метод проверки лицензии по трем параметрам - аккаунт, ключ и продукт.
        public override Task<LicenseCheckResponse> CheckLicense(LicenseCheckRequest request, ServerCallContext context)
        {
            Log.Information($"LicenseCheck получил запрос: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                // Поиск нужной лицензии.
                bool isExists = database.Licenses.Any(license =>
                    license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                // В случае, если она была найдена, возвращается сообщени об этом.
                if (isExists)
                    return Task.FromResult(LicenseCheckReplies.LicenseIsExists());
                // Иначе возвращается сообщение о том, что она не была найдена.
                else return Task.FromResult(LicenseCheckReplies.LicenseIsNotExists());
            }
        }
    }
}
