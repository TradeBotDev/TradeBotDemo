using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using Microsoft.EntityFrameworkCore;

namespace AccountGRPC
{
    public partial class AccountService : Account.AccountBase
    {
        // Метод выхода из аккаунта
        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"Logout получил запрос: SessionId - {request.SessionId}.");
            using (var database = new Models.AccountContext())
            {
                // В случае, если аккаунт не был найден среди вошедших, появляется сообщение об ошибке.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return Task.FromResult(LogoutReplies.AccountNotFound());

                // Получение информации о входе вместе с данными о пользователе.
                var loginInfo = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account).First();

                // Если отключено сохранение всех данных о биржах пользователя после выхода из аккаунта, происходит
                // их удаление из базы данных (только связанные с этим пользователем).
                if (!loginInfo.SaveExchangesAfterLogout)
                {
                    var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == loginInfo.Account.AccountId);
                    foreach (Models.ExchangeAccess exchange in exchanges)
                        database.ExchangeAccesses.Remove(exchange);
                }
                
                // Удаление аккаунта из списка вошедших.
                database.LoggedAccounts.Remove(loginInfo);
                database.SaveChanges();
            }
            // В случае успешного удаления аккаунта из списка, в который пользователь зашел,
            // сервер отвечает, что выход был успешно завершен.
            return Task.FromResult(LogoutReplies.SuccessfulLogout());
        }

        // Метод проверки валидности текущей сессии.
        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"IsValidSession получил запрос: SessionId - {request.SessionId}.");
            using (var database = new Models.AccountContext())
            {
                // Проверка на наличие вошедших пользователем с тем же Id сессии, что
                // предоставляется клиентом. Если есть - сессия валидна.
                if (database.LoggedAccounts.Any(account => account.SessionId == request.SessionId))
                    return Task.FromResult(IsValidSessionReplies.IsValid());
                // Если нет - сессия невалидна.
                else return Task.FromResult(IsValidSessionReplies.IsNotValid());
            }
        }

        // Метод получения информации о текущем пользователе по Id сессии.
        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"CurrentAccountData получил запрос: SessionId - {request.SessionId}.");
            using (var database = new Models.AccountContext())
            {
                // Производится проверка на то, является ли текущий пользователь вошедшим (по Id сессии).
                if (!database.LoggedAccounts.Any(account => account.SessionId == request.SessionId))
                    return Task.FromResult(CurrentAccountReplies.AccountNotFound());

                // Если текущий пользователь вошедший, то сервер возвращает данные этого пользователя.
                else
                {
                    // Получение данных вошедшего пользователя.
                    var login = database.LoggedAccounts
                        .Where(login => login.SessionId == request.SessionId)
                        .Include(account => account.Account).First();

                    // Формирование ответа.
                    var reply = CurrentAccountReplies.SuccessfulGettingAccountData(new AccountInfo
                    {
                        AccountId = login.Account.AccountId,
                        Email = login.Account.Email,
                    });
                    return Task.FromResult(reply);
                }
            }
        }
    }
}
