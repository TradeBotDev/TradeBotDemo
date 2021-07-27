﻿using AccountGRPC;
using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class LogoutTests
    {
        AccountService service = new AccountService();

        // Тестирование выхода из существующего аккаунта.
        [Fact]
        public void LogoutFromLoggedAccount()
        {
            State.loggedIn = new();

            var registerRequest = new RegisterRequest()
            {
                Email = "logoutAfterLogin@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            var reply = service.Register(registerRequest, null).
                ContinueWith(login => service.Login(loginRequest, null)).
                ContinueWith(logout => service.Logout(new SessionRequest
                { 
                    SessionId = logout.Result.Result.SessionId 
                }, null));

            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
        }

        // Тестирование выхода из несуществующего аккаунта.
        [Fact]
        public void LogoutFromNotLoggedAccount()
        {
            var request = new SessionRequest { SessionId = "non_existing_sessionId" };
            var reply = service.Logout(request, null);
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}