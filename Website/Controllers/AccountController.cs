﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using Serilog;

using Website.Models;
using TradeBot.Facade.FacadeService.v1;

namespace Website.Controllers
{
    // Все страницы в данном контроллере требуют авторизации для доступа к ним.
    [Authorize]
    public class AccountController : Controller
    {
        private ILogger logger;

        // Добавление Id сессии в конструктор логгера.
        public AccountController()
            => logger = Log.ForContext<AccountController>()
            .ForContext("Where", "Website");

        // Метод, показывающий страницу аккаунта с биржами при get-запросе.
        [Route("Account")]
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            logger = logger.ForContext("SessionId", User.Identity.Name)
                .ForContext("Method", nameof(Account))
                .ForContext<HttpGetAttribute>();

            logger.Information("{@Controller}: метод {@Method} принял запрос GET.", GetType().Name, "Account");

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
            logger = logger.ForContext("SessionId", User.Identity.Name)
                .ForContext("Method", nameof(Account))
                .ForContext<HttpPostAttribute>();

            logger.Information("{@Controller}: метод {@Method} принял запрос POST с данными: " +
                $"exchangeCode - {exchangeCode}.", GetType().Name, "Account");

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