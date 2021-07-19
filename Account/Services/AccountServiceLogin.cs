﻿using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;

namespace Account
{
    public partial class AccountService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        // Метод входа в аккаунт по запросу клиента.
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            // Валидация полей запроса
            (ActionCode, string) validationResult = Validate.LoginFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = "Отсутствует",
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext())
            {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accounts = database.Accounts.Where(accounts => accounts.Email == request.Email &&
                    accounts.Password == request.Password);

                // Проверка на наличие зарегистрированных аккаунтов с данными из запроса, и в
                // случае их отсутствия отправляет ответ с сообщением об ошибке.
                if (accounts.Count() == 0)
                {
                    return Task.FromResult(new LoginReply
                    {
                        SessionId = "Отсутствует",
                        Result = ActionCode.AccountNotFound,
                        Message = Messages.accountNotFound
                    });
                }

                // Проверка на то, есть ли сессия с пользователем, который пытается войти в аккаунт, и
                // в случае, если он вошел, возвращается его Id сессии
                foreach (KeyValuePair<string, Models.Account> account in loggedIn)
                {
                    if (request.Email == account.Value.Email)
                    {
                        return Task.FromResult(new LoginReply
                        {
                            SessionId = account.Key,
                            Result = ActionCode.Successful,
                            Message = Messages.alreadySignedIn
                        });
                    }
                }

                // В случае наличия зарегистрированного аккаунта с данными из запроса генерируется
                // Id сессии, а также полученный пользователь добавляется в коллекцию с вошедшими
                // пользователями.
                string sessionId = Guid.NewGuid().ToString();
                loggedIn.Add(sessionId, accounts.First());

                // Ответ сервера об успешном входе в аккаунт.
                return Task.FromResult(new LoginReply
                {
                    SessionId = sessionId,
                    Message = Messages.successfulLogin,
                    Result = ActionCode.Successful
                });
            }
        }
    }
}
