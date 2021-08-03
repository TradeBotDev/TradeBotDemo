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
        // �����, ����������� ���������� �������� ��� ������������� ������������ �� ������������ �������.
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            Log.Information($"SetLicense ������� ������: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                // ����� ������������ �������� �� ������ �������.
                bool licenseIsExists = database.Licenses.Any(license => license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                // � ������, ���� ����� ��� ������, ������������ ��������� � ���, ��� �������� ��� ����.
                if (licenseIsExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());
                else
                {
                    // ����� ��������� ����� �������� � ������������ � ���� ������.
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

        // ����� �������� �������� �� ���� ���������� - �������, ���� � �������.
        public override Task<LicenseCheckResponse> CheckLicense(LicenseCheckRequest request, ServerCallContext context)
        {
            Log.Information($"LicenseCheck ������� ������: AccountId - {request.AccountId}, Product - {request.Product}.");
            using (var database = new Models.LicenseContext())
            {
                // ����� ������ ��������.
                bool isExists = database.Licenses.Any(license =>
                    license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                // � ������, ���� ��� ���� �������, ������������ �������� �� ����.
                if (isExists)
                    return Task.FromResult(LicenseCheckReplies.LicenseIsExists());
                // ����� ������������ ��������� � ���, ��� ��� �� ���� �������.
                else return Task.FromResult(LicenseCheckReplies.LicenseIsNotExists());
            }
        }
    }
}
