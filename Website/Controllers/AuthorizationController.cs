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

namespace Website.Controllers
{
    public class AuthorizationController : Controller
    {
        private Account.AccountClient accountClient;

        public AuthorizationController()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            accountClient = new(channel);
        }

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
            var loginReply = accountClient.Login(new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            });

            if (loginReply.Result == AccountActionCode.Successful)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginReply.SessionId)
                };
                ClaimsIdentity id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
                return RedirectToAction("Account", "Account");
            }
            else
            {
                ViewBag.Title = "Произошла ошибка";
                ViewBag.SectionTitle = "Произошла ошибка";
                return View("~/Views/Error/Error.cshtml", loginReply.Message);
            }
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
            var registerReply = accountClient.Register(new RegisterRequest
            {
                Email = model.Email,
                Password = model.Password,
                VerifyPassword = model.VerifyPassword
            });

            if (registerReply.Result == AccountActionCode.Successful)
                return Content("Успешная регистрация");
            else
            {
                ViewBag.Title = "Произошла ошибка";
                ViewBag.SectionTitle = "Произошла ошибка";
                return View("~/Views/Error/Error.cshtml", registerReply.Message);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var logoutReply = accountClient.Logout(new LogoutRequest
            {
                SessionId = User.Identity.Name,
                SaveExchangeAccesses = false
            });

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (logoutReply.Result == AccountActionCode.Successful)
                return Content("Вы вышли");
            else
            {
                ViewBag.Title = "Произошла ошибка";
                ViewBag.SectionTitle = "Произошла ошибка";
                return View("~/Views/Error/Error.cshtml", logoutReply.Message);
            }
        }
    }
}
