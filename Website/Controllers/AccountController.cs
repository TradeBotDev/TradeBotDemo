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
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            var accountData = await Clients.AccountServiceClient.AccountData(User.Identity.Name);
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
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            var reply = await Clients.ExchangeAccessClient.DeleteExchangeAccess(User.Identity.Name, exchangeCode);
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public async Task<IActionResult> AddExchangeAccess()
        {
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddExchangeAccess(AddExchangeAccessModel model)
        {
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            if (!ModelState.IsValid)
                return View();

            var reply = await Clients.ExchangeAccessClient.AddExchangeAccess(User.Identity.Name, model);
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");
            else await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
