using Microsoft.AspNetCore.Mvc;
using TradeBot.Account.AccountService.v1;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            return View();
        }
    }
}
