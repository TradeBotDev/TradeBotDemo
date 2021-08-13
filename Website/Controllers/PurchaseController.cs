using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Buy()
        {
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        public async Task<IActionResult> Buy(CreditCardModel model)
        {
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            if (!ModelState.IsValid || haveLicense.HaveAccess)
                return View();

            var reply = await Clients.LicenseClient.SetLicense(User.Identity.Name, ProductCode.Tradebot, model);
            if (reply.Code == LicenseCode.Successful)
                return RedirectToAction("Account", "Account");
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
