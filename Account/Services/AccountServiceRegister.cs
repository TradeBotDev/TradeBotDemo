using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;

namespace Account
{
    public partial class AccountSystemService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        // Метод регистрации аккаунта по запросу клиента. Вход в аккаунт после регистрации НЕ производится!
        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            // Валидация полей запроса
            var validationResult = Validate.RegisterFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext()) {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accountsWithThisEmail = database.Accounts.Where(accounts => accounts.Email == request.Email);

                // В случае наличия аккаунтов с таким же Email-адресом, как в запросе, возвращается
                // ответ сервера с ошибкой, сообщающей об этом.
                if (accountsWithThisEmail.Count() > 0)
                {
                    return Task.FromResult(new RegisterReply
                    {
                        Result = ActionCode.AccountExists,
                        Message = Messages.accountExists
                    });
                }

                // В случае отсутствия пользователей с тем же Email-адресом, добавление в базу данных
                // нового пользователя с данными из базы данных.
                database.Add(new Models.Account()
                {
                    Email = request.Email,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password
                });
                // Сохранение изменений базы данных.
                database.SaveChanges();

                return Task.FromResult(new RegisterReply
                {
                    Result = ActionCode.Successful,
                    Message = Messages.successfulRegister
                });
            }
        }
    }
}
