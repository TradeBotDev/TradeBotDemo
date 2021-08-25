using AccountGRPC.LicenseMessages;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC
{
    public class LicenseService : License.LicenseBase
    {
        // Логгирование.
        protected readonly ILogger logger = Log.ForContext("Where", "AccountService");

        // Метод, который проверяет полученные данные и на их основе устанавливает лицензию для определенного пользователя.
        public override async Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"{request.SessionId}, " +
                $"{request.Product}, " +
                $"{request.CardNumber}, " +
                $"{request.Date}, " +
                $"{request.Cvv}.", GetType().Name, "SetLicense");

            using var database = new Models.AccountContext();

            // Получение текущего вошедшего аккаунта, включая объект, содержаний всю информацию о нем (для того,
            // чтобы в него можно было записать лицензию).
            var account = database.LoggedAccounts
                .Where(login => login.SessionId == request.SessionId)
                .Include(login => login.Account);

            // В случае, если такой пользователей среди вошедших не был найден, возвращается сообщение об этом.
            if (!account.Any())
                return await Task.FromResult(SetLicenseReplies.AccountNotFound());

            // Иначе происходит проверка на существование лицензии на добавляемый продукт, которая соответствует
            // найденному аккаунту.
            bool isExists = database.Licenses.Any(license => license.Product == request.Product &&
                license.Account.AccountId == account.First().AccountId);

            // В случае, если лицензия уже существует, возвращается сообщение об этом.
            if (isExists)
                return await Task.FromResult(SetLicenseReplies.LicenseIsExists());
            // Иначе в аккаунт добавляется новая лицензия и изменения записываются в базу данных.
            else
            {
                // Добавление лицензии в текущий аккаунт.
                account.First().Account.Licenses.Add(new Models.License { Product = request.Product });
                database.SaveChanges();

                // Сообщение о том, что лицензия была успешно добавлена в аккаунт.
                return await Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
            }
        }

        // Метод, который проверяет, существует ли лицензия у пользователя на данный продукт.
        public override async Task<CheckLicenseResponse> CheckLicense(CheckLicenseRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Product - {request.Product}.", GetType().Name, "CheckLicense");

            using var database = new Models.AccountContext();

            // Получение информации о входе текущего пользователя.
            var currentAccount = database.LoggedAccounts.Where(login => login.SessionId == request.SessionId);

            // Если пользователь не является вошедшим, возвращается сообщение о том, что аккаунт не существует.
            if (!currentAccount.Any())
                return await Task.FromResult(CheckLicenseReplies.AccountNotFound());

            // Иначе производится поиск информации о лицензии на продукт из запроса.
            var license = database.Licenses.Where(account => account.Account.AccountId == currentAccount.First().AccountId);

            // Если лицензия не была найдена, возвращается сообщение об этом.
            if (!license.Any())
                return await Task.FromResult(CheckLicenseReplies.LicenseIsNotExists());

            // Иначе возвращается сообщение о том, что пользователь обладает лицензией на этот продукт.
            return await Task.FromResult(CheckLicenseReplies.LicenseIsExists());
        }
    }
}
