using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [Route("Account")]
        public IActionResult Account()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var accountClient = new Account.AccountClient(channel);
            var exchangeClient = new ExchangeAccess.ExchangeAccessClient(channel);

            var accountData = accountClient.AccountData(new AccountDataRequest
            {
                SessionId = User.Identity.Name
            });

            if (accountData.Result == AccountActionCode.Successful)
            {

                var allExchanges = exchangeClient.AllExchangesBySession(new AllExchangesBySessionRequest
                {
                    SessionId = User.Identity.Name
                });

                var model = new AccountPageModel
                {
                    Email = accountData.CurrentAccount.Email,
                    Exchanges = allExchanges.Exchanges
                };

                ViewBag.Title = "Аккаунт";
                ViewBag.SectionTitle = "Аккаунт";
                return View(model);
            }
            else return View("~/Views/Shared/Error.cshtml", accountData.Message);
        }

        [HttpGet]
        public IActionResult AddExchangeAccess()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddExchangeAccess(AddExchangeAccessModel model)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var exchangeClient = new ExchangeAccess.ExchangeAccessClient(channel);

            ExchangeAccessCode exchangeAccessCode;
            switch (model.SelectExchange)
            {
                case "Bitmex":
                    exchangeAccessCode = ExchangeAccessCode.Bitmex;
                    break;
                default:
                    exchangeAccessCode = ExchangeAccessCode.Unspecified;
                    break;
            }

            var reply = exchangeClient.AddExchangeAccess(new AddExchangeAccessRequest
            {
                SessionId = User.Identity.Name,
                Code = exchangeAccessCode,
                ExchangeName = model.SelectExchange,
                Token = model.Token,
                Secret = model.Secret
            });

            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
