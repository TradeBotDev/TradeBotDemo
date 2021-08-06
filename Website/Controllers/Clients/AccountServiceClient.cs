using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;
using Website.Models.Authorization;

namespace Website.Controllers.Clients
{
    public static class AccountServiceClient
    {
        private static Account.AccountClient client = new(GrpcChannel.ForAddress("http://localhost:5000"));

        public static LoginResponse Login(LoginModel model)
        {
            var request = new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            };
            return client.Login(request);
        }

        public static LogoutResponse Logout(string sessionId, bool saveExchangeAccesses)
        {
            var request = new LogoutRequest
            {
                SessionId = sessionId,
                SaveExchangeAccesses = saveExchangeAccesses
            };
            return client.Logout(request);
        }

        public static RegisterResponse Register(RegisterModel model)
        {
            var request = new RegisterRequest
            {
                Email = model.Email,
                Password = model.Password,
                VerifyPassword = model.VerifyPassword
            };
            return client.Register(request);
        }

        public static IsValidSessionResponse IsValidSession(string sessionId)
        {
            var request = new IsValidSessionRequest
            {
                SessionId = sessionId
            };
            return client.IsValidSession(request);
        }

        public static AccountDataResponse AccountData(string sessionId)
        {
            var request = new AccountDataRequest
            {
                SessionId = sessionId
            };
            return client.AccountData(request);
        }

        public static CheckLicenseResponse CheckLicense(string sessionId)
        {
            var request = new CheckLicenseRequest
            {
                SessionId = sessionId
            };
            return client.CheckLicense(request);
        }
    }
}
