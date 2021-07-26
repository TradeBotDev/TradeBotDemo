using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.AccountMessages;
using Account.Models;

namespace Account
{
    public partial class AccountService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        private readonly ILogger<AccountService> _logger;

        public AccountService(ILogger<AccountService> logger) => _logger = logger;

        // Метод выхода из аккаунта
        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            // В случае, если аккаунт не был найден среди вошедших, появляется сообщение об ошибке.
            if (!State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(LogoutReplies.AccountNotFound);

            // Необходимые данные для удаления бирж.
            int accountId = State.loggedIn[request.SessionId].AccountId;
            bool saveExchanges = State.loggedIn[request.SessionId].SaveExchangesAfterLogout;

            // Если отключено сохранение всех данных о биржах пользователя после выхода из аккаунта, происходит
            // их удаление из базы данных (только связанные с этим пользователем).
            if (!saveExchanges)
            {
                using (var database = new AccountContext())
                {
                    var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == accountId);
                    foreach (Models.ExchangeAccess exchange in exchanges)
                        database.ExchangeAccesses.Remove(exchange);
                    database.SaveChanges();
                }
            }
            // Удаление аккаунта из списка вошедших и запись изменений в файл.
            State.loggedIn.Remove(request.SessionId);
            FileManagement.WriteFile(State.LoggedInFilename, State.loggedIn);

            // В случае успешного удаления аккаунта из списка, в который пользователь зашел,
            // сервер отвечает, что выход был успешно завершен.
            return Task.FromResult(LogoutReplies.SuccessfulLogout);
        }

        // Метод проверки валидности текущей сессии.
        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            // Проверка на наличие вошедших пользователем с тем же Id сессии, что
            // предоставляется клиентом. Если есть - сессия валидна, нет - невалидна.
            if (State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(IsValidSessionReplies.IsValid);
            else return Task.FromResult(IsValidSessionReplies.IsNotValid);
        }

        // Метод получения информации о текущем пользователе по Id сессии.
        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            // Производится проверка на то, является ли текущий пользователь вошедшим (по Id сессии).
            if (!State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(CurrentAccountReplies.AccountNotFound);
            // Если текущий пользователь вошедший, то сервер возвращает данные этого пользователя.
            else
            {
                using (var database = new AccountContext())
                {
                    // Получение данных вошедшего пользователя.
                    Models.Account account = database.Accounts.Where(id => id.AccountId == State.loggedIn[request.SessionId].AccountId).First(); 
                    return Task.FromResult(CurrentAccountReplies.SuccessfulGettingAccountData(new AccountInfo
                    {
                        AccountId = account.AccountId,
                        Email = account.Email,
                    }));
                }
            }
        }
    }
}
