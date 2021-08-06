﻿using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [Route("Account")]
        [HttpGet]
        public IActionResult Account()
        {
            var accountData = Clients.AccountServiceClient.AccountData(User.Identity.Name);
            if (accountData.Result == AccountActionCode.Successful)
            {
                var model = new AccountDataModel
                {
                    Email = accountData.CurrentAccount.Email,
                    Exchanges = accountData.CurrentAccount.Exchanges
                };

                ViewBag.Title = "Аккаунт";
                ViewBag.SectionTitle = "Аккаунт";
                return View(model);
            }
            else return View("~/Views/Shared/Error.cshtml", accountData.Message);
        }

        [Route("Account")]
        [HttpPost]
        public IActionResult Account(ExchangeAccessCode exchangeCode)
        {
            var reply = Clients.ExchangeAccessClient.DeleteExchangeAccess(User.Identity.Name, exchangeCode);
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public IActionResult AddExchangeAccess()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddExchangeAccess(AddExchangeAccessModel model)
        {
            var reply = Clients.ExchangeAccessClient.AddExchangeAccess(User.Identity.Name, model);

            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
