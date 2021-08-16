using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        // Метод, показывающий главную страницу сайта.
        public async Task<IActionResult> Index()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }
    }
}
