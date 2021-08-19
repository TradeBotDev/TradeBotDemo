using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    // Все страницы в данном контроллере требуют авторизации для доступа к ним.
    [Authorize]
    public class AccountController : Controller
    {
        // Метод, показывающий страницу аккаунта с биржами при get-запросе.
        [Route("Account")]
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Получение информации об аккаунте по текущему Id сессии.
            var accountData = await Clients.AccountServiceClient.AccountData(User.Identity.Name);
            // Если информация была получена успешно, создается новый объект модели, который передается в
            // представление и открывается страница аккаунта.
            if (accountData.Result == AccountActionCode.Successful)
            {
                var model = new AccountDataModel
                {
                    Email = accountData.CurrentAccount.Email,
                    Exchanges = accountData.CurrentAccount.Exchanges
                };
                return View(model);
            }
            // Иначе происходит выход из аккаунта и появляется страница с сообщением об ошибке, которую
            // вернул сервис AccountService.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Shared/Error.cshtml", accountData.Message);
        }

        // Метод, который удаляет биржу после отправки формы (кнопка на карточке) на странице аккаунта.
        [Route("Account")]
        [HttpPost]
        public async Task<IActionResult> Account(ExchangeAccessCode exchangeCode)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Запрос на удаление биржи в ExchangeAccessService.
            var reply = await Clients.ExchangeAccessClient.DeleteExchangeAccess(User.Identity.Name, exchangeCode);
            // Если биржа была успешно удалена, происходит перенаправление пользователя на ту же страницу.
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");

            // Иначе происходит выход из аккаунта и появляется страница с сообщением об ошибке.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
