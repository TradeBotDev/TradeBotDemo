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
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            if (!ModelState.IsValid)
                return View();

            var reply = Clients.AccountServiceClient.Login(model);
            if (reply.Result == AccountActionCode.Successful)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, reply.SessionId) };
                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Account", "Account");
            }
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            if (!ModelState.IsValid)
                return View();

            var reply = Clients.AccountServiceClient.Register(model);
            if (reply.Result == AccountActionCode.Successful)
            {
                return await Login(new LoginModel
                {
                    Email = model.Email,
                    Password = model.Password
                });
            }
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();
            if (!User.Identity.IsAuthenticated)
                return View("~/Views/Shared/Error.cshtml", "Вы уже вышли.");
            else return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout(LogoutModel model)
        {
            ViewBag.HaveLicense = Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot).HaveAccess;
            if (model.Button == "Выйти")
            {
                var reply = Clients.AccountServiceClient.Logout(User.Identity.Name, model.SaveExchanges);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (reply.Result != AccountActionCode.Successful)
                    return View("~/Views/Shared/Error.cshtml", reply.Message);
            }
            return Redirect(model.PreviousUrl);
        }
    }
}
