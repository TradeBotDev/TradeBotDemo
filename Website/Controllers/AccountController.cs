using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [Route("Account")]
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            var accountData = Clients.AccountServiceClient.AccountData(User.Identity.Name);
            if (accountData.Result == AccountActionCode.Successful)
            {
                var model = new AccountDataModel
                {
                    Email = accountData.CurrentAccount.Email,
                    Exchanges = accountData.CurrentAccount.Exchanges
                };

                return View(model);
            }
            else await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Shared/Error.cshtml", accountData.Message);
        }

        [Route("Account")]
        [HttpPost]
        public async Task<IActionResult> Account(ExchangeAccessCode exchangeCode)
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            var reply = Clients.ExchangeAccessClient.DeleteExchangeAccess(User.Identity.Name, exchangeCode);
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public async Task<IActionResult> AddExchangeAccess()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddExchangeAccess(AddExchangeAccessModel model)
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            if (!ModelState.IsValid)
                return View();

            var reply = Clients.ExchangeAccessClient.AddExchangeAccess(User.Identity.Name, model);
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
