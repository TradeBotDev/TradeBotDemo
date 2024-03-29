﻿using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class DeleteExchangeAccessTests : ExchangeAccessServiceTestsData
    {
        // Тестирование удаления несуществующей информации о доступе к бирже из существующего аккаунта.
        [Fact]
        public void DeleteNotExistingExchangeAccessTest()
        {
            // Последовательная регистрация, вход и удаление информации о доступе к бирже.
            var reply = GenerateLogin("not_existing_exchange_deletion").ContinueWith(loginReply =>
                exchangeAccessService.DeleteExchangeAccess(new DeleteExchangeAccessRequest
                {
                    SessionId = loginReply.Result.SessionId,
                    Code = ExchangeAccessCode.Bitmex
                }, null));

            // Ожидается что в результате не будет найдена информация о бирже.
            Assert.Equal(ExchangeAccessActionCode.IsNotFound, reply.Result.Result.Result);
        }

        // Тестирование на удаление информации о доступе к бирже из несуществующего аккаунта.
        [Fact]
        public void DeleteExchangeAccessFromNonExistingAccountTest()
        {
            // Запрос с заведомо несуществующим аккаунтом.
            var request = new DeleteExchangeAccessRequest
            {
                Code = ExchangeAccessCode.Bitmex,
                SessionId = "non_existing_session_id"
            };
            var reply = exchangeAccessService.DeleteExchangeAccess(request, null);

            // Ожидается, что в результате аккаунт не будет найден.
            Assert.Equal(ExchangeAccessActionCode.AccountNotFound, reply.Result.Result);
        }

        // Тестирование на удаление существующей информации из существующего аккаунта.
        [Fact]
        public void DeleteExistingExchangeAccessTest()
        {
            string sessionId = "none";

            // Локальный метод, генерирующий запрос на добавление информации о доступе к бирже
            // и записывающий Id сессии в переменную.
            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    Code = ExchangeAccessCode.Bitmex,
                    ExchangeName = "Bitmex",
                    SessionId = sessionId,
                    Token = "test_token",
                    Secret = "test_secret"
                };
            }

            // Последовательная генерация входа в аккаунт, добавление информации о доступе к биржи в аккаунт,
            // а затем ее удаление.
            var reply = GenerateLogin("delete_existing_exchange")
                .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(GenerateRequest(loginReply.Result.SessionId), null))
                .ContinueWith(none => exchangeAccessService.DeleteExchangeAccess(new DeleteExchangeAccessRequest
                {
                    SessionId = sessionId,
                    Code = ExchangeAccessCode.Bitmex
                }, null));

            // Ожидается, что удаление информации будет завершено успешно.
            Assert.Equal(ExchangeAccessActionCode.Successful, reply.Result.Result.Result);
        }
    }
}
