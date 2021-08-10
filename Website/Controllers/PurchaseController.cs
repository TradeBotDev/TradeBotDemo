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
            return View();
        }

        public IActionResult Buy(CreditCardModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var reply = Clients.LicenseClient.SetLicense(User.Identity.Name, ProductCode.Tradebot, model);
            if (reply.Code == LicenseCode.Successful)
                return Content(reply.Message);
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
