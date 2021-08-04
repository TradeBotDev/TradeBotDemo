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
        public IActionResult Login(string email, string password)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var accountClient = new Account.AccountClient(channel);

            var loginReply = accountClient.Login(new LoginRequest
            {
                Email = email,
                Password = password
            });

            if (loginReply.Result == AccountActionCode.Successful)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, "session_id")
                };
                ClaimsIdentity id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
                return RedirectToAction("Account", "Account");
            }
            else
            {
                ViewBag.Title = "Произошла ошибка";
                ViewBag.SectionTitle = "Ошибка при входе";
                return View("Failed", loginReply.Message);
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
        public IActionResult Register(string email, string password, string verify_password)
        {
            ViewBag.Title = "Произошла ошибка";
            ViewBag.SectionTitle = "Ошибка при регистрации";
            return View("Failed", "Какой-то текст");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var accountClient = new Account.AccountClient(channel);
            var logoutReply = accountClient.Logout(new LogoutRequest
            {
                SessionId = User.Identity.Name,
                SaveExchangeAccesses = false
            });
            if (logoutReply.Result == AccountActionCode.Successful)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Content("Вы вышли");
            }
            else
            {
                ViewBag.Title = "Произошла ошибка";
                ViewBag.SectionTitle = "Ошибка при выходе";
                return View("Failed", logoutReply.Message);
            }
        }
    }
}
