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
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            Log.Information($"SetLicense получил запрос: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                bool licenseIsExists = database.Licenses.Any(license => license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                if (licenseIsExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());
                else
                {
                    var license = new Models.License
                    {
                        AccountId = request.AccountId,
                        Key = Guid.NewGuid().ToString(),
                        Product = request.Product
                    };

                    database.Licenses.Add(license);
                    database.SaveChanges();
                    return Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
                }
            }
        }

        public override Task<LicenseCheckResponse> LicenseCheck(LicenseCheckRequest request, ServerCallContext context)
        {
            Log.Information($"LicenseCheck получил запрос: AccountId - {request.AccountId}, Key - {request.Key}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                bool isExists = database.Licenses.Any(license =>
                    license.AccountId == request.AccountId &&
                    license.Key == request.Key &&
                    license.Product == request.Product);

                if (isExists)
                    return Task.FromResult(LicenseCheckReplies.LicenseIsExists());
                else return Task.FromResult(LicenseCheckReplies.LicenseIsNotExists());
            }
        }

        public override Task<GetKeyResponse> GetKey(GetKeyRequest request, ServerCallContext context)
        {
            Log.Information($"GetKey получил запрос: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                var license = database.Licenses.Where(license =>
                    license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                if (license.Count() > 0)
                {
                    string key = license.First().Key;
                    return Task.FromResult(GetKeyReplies.LicenseIsExists(key));
                }
                else return Task.FromResult(GetKeyReplies.LicenseIsNotExists());
            }
        }
    }
}
