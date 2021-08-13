using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers
{
    // Все страницы в данном контроллере требуют авторизации для того, чтобы к ним был доступ.
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

        // Метод, показывающий форму добавления биржи в аккаунт.
        [HttpGet]
        public async Task<IActionResult> AddExchangeAccess()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        // Метод, который добавляет биржу после отправки формы добавления биржи.
        [HttpPost]
        public async Task<IActionResult> AddExchangeAccess(AddExchangeAccessModel model)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            // Если данные модели не являются валидными, возвращается страница формы с сообщениями об ошибках.
            if (!ModelState.IsValid)
                return View();

            // Иначе отправляется запрос на добавление биржи в аккаунт.
            var reply = await Clients.ExchangeAccessClient.AddExchangeAccess(User.Identity.Name, model);
            // Если биржа была успешно добавлена, происходит перенаправление на страницу аккаунта, где она будет отображаться.
            if (reply.Result == ExchangeAccessActionCode.Successful)
                return RedirectToAction("account", "account");

            // Иначе если проблема при добавлении связана с аккаунтом, происходит выход из него.
            if (reply.Result == ExchangeAccessActionCode.AccountNotFound || reply.Result == ExchangeAccessActionCode.TimePassed)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Возвращение страницы с ошибкой.
            return View("~/Views/Shared/Error.cshtml", reply.Message);
        }
    }
}
