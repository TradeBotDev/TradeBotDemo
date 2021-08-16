using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    // Все страницы в данном контроллере требуют авторизации для доступа к ним.
    [Authorize]
    public class PurchaseController : Controller
    {
        // Метод, показывающий страницу покупки приложения.
        [HttpGet]
        public async Task<IActionResult> Buy()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        public async Task<IActionResult> Buy(CreditCardModel model)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Если данные модели являются невалидными, возвращается та же страница с сообщениями об ошибке.
            if (!ModelState.IsValid)
                return View();

            // Отправка запроса на получение лицензии.
            var reply = await Clients.LicenseClient.SetLicense(User.Identity.Name, ProductCode.Tradebot, model);

            // Если лицензия была получена успешно, происходит перенаправление на страницу аккаунта.
            if (reply.Code == LicenseCode.Successful)
                return RedirectToAction("Account", "Account");
            // Иначе возвращается страница с сообщением об ошибке.
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
