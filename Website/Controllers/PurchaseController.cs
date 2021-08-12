using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        [HttpGet]
        public IActionResult Buy()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            return View();
        }

        public IActionResult Buy(CreditCardModel model)
        {
            bool haveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            ViewBag.HaveLicense = haveLicense;

            if (!ModelState.IsValid || haveLicense)
                return View();

            var reply = Clients.LicenseClient.SetLicense(User.Identity.Name, ProductCode.Tradebot, model);
            if (reply.Code == LicenseCode.Successful)
                return RedirectToAction("Account", "Account");
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
