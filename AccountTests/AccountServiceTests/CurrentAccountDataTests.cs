﻿using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    [Collection("AccountTests")]
    public class CurrentAccountDataTests : AccountServiceTestsData
    {
        // Тестирование получения данных из существующего аккаунта.
        [Fact]
        public void DataFromExistingAccount()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"existing_user@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password,
            };

            // Последовательная регистрация аккаунта, вход и получение его данных.
            var reply = service.Register(registerRequest, null)
            .ContinueWith(login => service.Login(loginRequest, null))
            .ContinueWith(login => service.AccountData(
                new AccountDataRequest { SessionId = login.Result.Result.SessionId }, null));

            // Ожидается, что получение данных аккаунта будет успешно завершено.
            Assert.Equal(AccountActionCode.Successful, reply.Result.Result.Result);
        }

        // Тестирование получения данных из несуществующего аккаунта.
        [Fact]
        public void DataFromNonExistingAccount()
        {
            // Намеренное отправление несуществующего id сессии в метод CurrentAccountData.
            var request = new AccountDataRequest { SessionId = "non_existing_session_id" };
            var reply = service.AccountData(request, null);

            // Ожидается, что в качествет ответа придет сообщение о том, что аккаунт не был найден.
            Assert.Equal(AccountActionCode.IsNotFound, reply.Result.Result);
        }
    }
}
