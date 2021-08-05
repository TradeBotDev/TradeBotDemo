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
            var reply = accountClient.AccountData(new AccountDataRequest
            {
                SessionId = User.Identity.Name
            });

            var model = new AccountDataModel
            {
                Email = reply.CurrentAccount.Email,
                Exchanges = reply.CurrentAccount.Exchanges
            };

            ViewBag.Title = "Аккаунт";
            ViewBag.SectionTitle = "Аккаунт";
            return View(model);
        }
    }
}
