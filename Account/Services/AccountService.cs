using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;
using System.Web;

namespace Account
{
    public class AccountService : AcountService.AcountServiceBase
    {
        private readonly ILogger<AccountService> _logger;

        private Dictionary<string, Models.Account> loggedIn;

        public AccountService(ILogger<AccountService> logger)
        {
            _logger = logger;
            loggedIn = new Dictionary<string, Models.Account>();
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var validationResult = Validate.LoginFields(request);

            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = "Отсутствует",
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            using (var database = new Models.AccountContext())
            {
                var accounts = database.Accounts.Where(accounts => accounts.Email == request.Email && accounts.Password == request.Password);
                
                if (accounts.Count() == 0)
                    return Task.FromResult(new LoginReply
                    {
                        SessionId = "Отсутствует",
                        Result = ActionCode.AccountNotFound,
                        Message = Messages.accountNotFound
                    });

                string sessionId = Guid.NewGuid().ToString();
                loggedIn.Add(sessionId, accounts.First());

                return Task.FromResult(new LoginReply
                {
                    SessionId = sessionId,
                    Message = Messages.successfulLogin,
                    Result = ActionCode.Successful
                });
            }
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            var validationResult = Validate.RegisterFields(request);

            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            using (var database = new Models.AccountContext()) {
                var accountsWithThisEmail = database.Accounts.Where(accounts => accounts.Email == request.Email);

                if (accountsWithThisEmail.Count() > 0)
                    return Task.FromResult(new RegisterReply
                    {
                        Result = ActionCode.AccountExists,
                        Message = Messages.accountExists
                    });

                database.Add(new Models.Account()
                {
                    Email = request.Email,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password
                });
                database.SaveChanges();

                return Task.FromResult(new RegisterReply
                {
                    Result = ActionCode.Successful,
                    Message = Messages.successfulRegister
                });
            }
        }

        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            if (loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(new SessionReply
                {
                    IsValid = true,
                    Message = Messages.isValid
                });
            else return Task.FromResult(new SessionReply
            {
                IsValid = false,
                Message = Messages.isNotValid
            });
        }

        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            if (!loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(new CurrentAccountReply
                {
                    Result = ActionCode.AccountNotFound,
                    Message = Messages.accountNotFound,
                    CurrentAccount = new AccountInfo()
                });
            else return Task.FromResult(new CurrentAccountReply
            {
                Result = ActionCode.Successful,
                Message = Messages.successfulOperation,
                CurrentAccount = new AccountInfo
                {
                    Id = loggedIn[request.SessionId].AccountId,
                    Firstname = loggedIn[request.SessionId].Firstname,
                    Lastname = loggedIn[request.SessionId].Lastname,
                    Email = loggedIn[request.SessionId].Email,
                    PhoneNumber = loggedIn[request.SessionId].PhoneNumber
                }
            });
        }
    }
}
