using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;
using Website.Models.Authorization;

namespace Website.Controllers
{
    public class AuthorizationController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Title = "Вход";
            ViewBag.SectionTitle = "Вход";
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            var reply = Clients.AccountServiceClient.Login(model);
            if (reply.Result == AccountActionCode.Successful)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, reply.SessionId) };
                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Account", "Account");
            }
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Title = "Регистрация";
            ViewBag.SectionTitle = "Регистрация";
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            var reply = Clients.AccountServiceClient.Register(model);
            if (reply.Result == AccountActionCode.Successful)
            {
                return Login(new LoginModel
                {
                    Email = model.Email,
                    Password = model.Password
                });
            }
            else return View("~/Views/Shared/Error.cshtml", reply.Message);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();

            if (!User.Identity.IsAuthenticated)
                return View("~/Views/Shared/Error.cshtml", "Вы уже вышли.");

            ViewBag.Title = "Выход из аккаунта";
            ViewBag.SectionTitle = "Выход из аккаунта";
            return View();
        }

        [HttpPost]
        public IActionResult Logout(LogoutModel model)
        {
            if (model.Button == "Выйти")
            {
                var reply = Clients.AccountServiceClient.Logout(User.Identity.Name, model.SaveExchanges);
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (reply.Result != AccountActionCode.Successful)
                    return View("~/Views/Shared/Error.cshtml", reply.Message);
            }
            return Redirect(model.PreviousUrl);
        }
    }
}
