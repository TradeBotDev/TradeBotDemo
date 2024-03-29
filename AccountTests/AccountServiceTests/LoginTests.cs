﻿using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;
using System.Linq;

namespace AccountTests.AccountServiceTests
{
    [Collection("AccountTests")]
    public class LoginTests : AccountServiceTestsData
    {
        // Тестирование входа в существующий аккаунт.
        [Fact]
        public void LoginToExistingAccount()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"login_to_existing_user@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = "password"
            };
            // Регистрация тестового аккаунта для того, чтобы потом в него можно было войти, а также
            // вход в аккаунт.
            var reply = service.Register(registerRequest, null);
            reply.ContinueWith(login => service.Login(loginRequest, null));

            using (var database = new AccountContext())
            {
                var accounts = database.Accounts.Where(account => account.Email == loginRequest.Email);
                database.Accounts.Remove(accounts.First());
                database.SaveChanges();
            }

            // Ожидается, что вход в аккаунт будет успешным.
            Assert.Equal(AccountActionCode.Successful, reply.Result.Result);
        }

        // Тестирование входа в несуществующий аккаунт.
        [Fact]
        public void LoginToNonExistingAccount()
        {
            // Генерация данных несуществующего аккауниа.
            var request = new LoginRequest()
            {
                Email = $"login_to_non_existing_user@pochta.ru",
                Password = "password"
            };
            var reply = service.Login(request, null);
            //Ожидается, что аккаунт не будет найден.
            Assert.Equal(AccountActionCode.IsNotFound, reply.Result.Result);
        }

        // Тестирование входа в уже вошедший аккаунт.
        [Fact]
        public void DoubleLogin()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"double_login_user@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            // Последовательная регистрация, вход в аккаунт и вход в тот же самый аккаунт (т.е. попытка входа
            // в аккаунт, когда пользователь уже является вошедшим).
            var reply = service.Register(registerRequest, null)
                .ContinueWith(login => service.Login(loginRequest, null))
                .ContinueWith(login => service.Login(loginRequest, null));

            // Ожидается, что в результате придет сообщение о том, что пользователь уже вошел, однако вход будет
            // считаться успешным и выдастся уже существующий id сессии.
            Assert.Equal(AccountActionCode.Successful, reply.Result.Result.Result);
        }
    }
}
