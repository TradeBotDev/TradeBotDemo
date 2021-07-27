using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;

namespace AccountGRPC
{
    public partial class AccountService : Account.AccountBase
    {
        // Метод выхода из аккаунта
        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"Logout получил запрос: SessionId - {request.SessionId}.");

            // В случае, если аккаунт не был найден среди вошедших, появляется сообщение об ошибке.
            if (Models.State.loggedIn == null || !Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(LogoutReplies.AccountNotFound());

            // Необходимые данные для удаления бирж.
            int accountId = Models.State.loggedIn[request.SessionId].AccountId;
            bool saveExchanges = Models.State.loggedIn[request.SessionId].SaveExchangesAfterLogout;

            // Если отключено сохранение всех данных о биржах пользователя после выхода из аккаунта, происходит
            // их удаление из базы данных (только связанные с этим пользователем).
            if (!saveExchanges)
            {
                using (var database = new Models.AccountContext())
                {
                    var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == accountId);
                    foreach (Models.ExchangeAccess exchange in exchanges)
                        database.ExchangeAccesses.Remove(exchange);
                    database.SaveChanges();
                }
            }
            // Удаление аккаунта из списка вошедших и запись изменений в файл.
            Models.State.loggedIn.Remove(request.SessionId);
            FileManagement.WriteFile(Models.State.LoggedInFilename, Models.State.loggedIn);

            // В случае успешного удаления аккаунта из списка, в который пользователь зашел,
            // сервер отвечает, что выход был успешно завершен.
            return Task.FromResult(LogoutReplies.SuccessfulLogout());
        }

        // Метод проверки валидности текущей сессии.
        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"IsValidSession получил запрос: SessionId - {request.SessionId}.");

            // Проверка на наличие вошедших пользователем с тем же Id сессии, что
            // предоставляется клиентом. Если есть - сессия валидна.
            if (Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(IsValidSessionReplies.IsValid());
            // Если нет - сессия невалидна.
            return Task.FromResult(IsValidSessionReplies.IsNotValid());
        }

        // Метод получения информации о текущем пользователе по Id сессии.
        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"CurrentAccountData получил запрос: SessionId - {request.SessionId}.");

            // Производится проверка на то, является ли текущий пользователь вошедшим (по Id сессии).
            if (!Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(CurrentAccountReplies.AccountNotFound());

            // Если текущий пользователь вошедший, то сервер возвращает данные этого пользователя.
            else
            {
                using (var database = new Models.AccountContext())
                {
                    // Получение данных вошедшего пользователя.
                    Models.Account account = database.Accounts.Where(id => id.AccountId == Models.State.loggedIn[request.SessionId].AccountId).First();
                    // Формирование ответа.
                    var reply = CurrentAccountReplies.SuccessfulGettingAccountData(new AccountInfo
                    {
                        AccountId = account.AccountId,
                        Email = account.Email,
                    });
                    return Task.FromResult(reply);
                }
            }
        }
    }
}
