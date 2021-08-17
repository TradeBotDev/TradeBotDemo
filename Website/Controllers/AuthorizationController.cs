using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models.Authorization;

namespace Website.Controllers
{
    public class AuthorizationController : Controller
    {
        // Метод, показывающий страницу входа в аккаунт.
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        // Метод, который производит вход в аккаунт в ответ на отправку формы.
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Если данные модели не являются валидными, возвращается страница формы с сообщениями об ошибках.
            if (!ModelState.IsValid)
                return View();

            // Запрос на вход в аккаунт по данным модели.
            var reply = await Clients.AccountServiceClient.Login(model);
            // Если вход в аккаунт был успешно завершен, производится аутентификация.
            if (reply.Result == AccountActionCode.Successful)
            {
                // Добавление данных, которые добавляются в Cookie.
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, reply.SessionId) };
                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                // Аутентификация и отправка клиенту аутентификационных Cookie.
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Account", "Account");
            }
            // Иначе возвращается страница с сообщением об ошибке.
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        // Метод, показывающий страницу регистрации.
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }

        // Метод, который производит регистрацию в ответ на отправку формы.
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Если данные модели не являются валидными, возвращается страница формы с сообщениями об ошибках.
            if (!ModelState.IsValid)
                return View();

            // Отправка запроса на регистрацию.
            var reply = await Clients.AccountServiceClient.Register(model);

            // Если регистрация была завершена успешно, производится вход в этот новый аккаунт (при котором
            // произойдет перенаправление на страницу аккаунта).
            if (reply.Result == AccountActionCode.Successful)
            {
                return await Login(new LoginModel
                {
                    Email = model.Email,
                    Password = model.Password
                });
            }
            // Иначе возвращается страница с сообщением об ошибке.
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        // Метод, показывающий страницу выхода из аккаунта.
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // В представление передается предыдущий url, чтобы при post-запросе возвращаться не на эту же страницу, а
            // на действительно предыдущую.
            ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();

            // Если пользователь уже вышел, вместо формы выхода отображается сообщение о том, что пользователь уже вышел.
            if (!User.Identity.IsAuthenticated)
                return View("~/Views/Shared/Error.cshtml", "Вы уже вышли.");
            // Иначе возвращается представление с формой выхода.
            else return View();
        }

        // Метод, который производит выход из аккаунта в ответ на отправку формы.
        [HttpPost]
        public async Task<IActionResult> Logout(LogoutModel model)
        {
            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;

            // Запрос на выход из аккаунта.
            var reply = await Clients.AccountServiceClient.Logout(User.Identity.Name, model.SaveExchanges);
            // Удаление аутентификационных куки пользователя.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Если выход не был завершен успешно, возвращается страница с сообщением об ошибке.
            if (reply.Result != AccountActionCode.Successful)
                return View("~/Views/Shared/Error.cshtml", reply.Message);
            
            // Иначе происходит перенаправление на предыдущий url.
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
