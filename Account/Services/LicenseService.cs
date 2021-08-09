using AccountGRPC.LicenseMessages;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC
{
    public class LicenseService : License.LicenseBase
    {
        // Метод, который проверяет полученные данные и на их основе устанавливает лицензию для определенного пользователя.
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            using (var database = new Models.AccountContext())
            {
                // Получение текущего вошедшего аккаунта, включая объект, содержаний всю информацию о нем (для того,
                // чтобы в него можно было записать лицензию).
                var account = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(login => login.Account);

                // В случае, если такой пользователей среди вошедших не был найден, возвращается сообщение об этом.
                if (account.Count() == 0)
                    return Task.FromResult(SetLicenseReplies.AccountNotFound());

                // Иначе происходит проверка на существование лицензии на добавляемый продукт, которая соответствует
                // найденному аккаунту.
                bool isExists = database.Licenses.Any(license => license.Product == request.Product &&
                    license.Account.AccountId == account.First().AccountId);

                // В случае, если лицензия уже существует, возвращается сообщение об этом.
                if (isExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());
                // Иначе в аккаунт добавляется новая лицензия и изменения записываются в базу данных.
                else
                {
                    // Добавление лицензии в текущий аккаунт.
                    account.First().Account.Licenses.Add(new Models.License { Product = request.Product });
                    database.SaveChanges();

                    // Сообщение о том, что лицензия была успешно добавлена в аккаунт.
                    return Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
                }
            }
        }

        // Метод, который проверяет, существует ли лицензия у пользователя на данный продукт.
        public override Task<CheckLicenseResponse> CheckLicense(CheckLicenseRequest request, ServerCallContext context)
        {
            using (var database = new Models.AccountContext())
            {
                // Получение информации о входе текущего пользователя.
                var currentAccount = database.LoggedAccounts.Where(login => login.SessionId == request.SessionId);

                // Если пользователь не является вошедшим, возвращается сообщение о том, что аккаунт не существует.
                if (currentAccount.Count() == 0)
                    return Task.FromResult(CheckLicenseReplies.AccountNotFound());

                // Иначе производится поиск информации о лицензии на продукт из запроса.
                var license = database.Licenses.Where(account => account.Account.AccountId == currentAccount.First().AccountId);

                // Если лицензия не была найдена, возвращается сообщение об этом.
                if (license.Count() == 0)
                    return Task.FromResult(CheckLicenseReplies.LicenseIsNotExists());

                // Иначе возвращается сообщение о том, что пользователь обладает лицензией на этот продукт.
                return Task.FromResult(CheckLicenseReplies.LicenseIsExists());
            }
        }
    }
}
