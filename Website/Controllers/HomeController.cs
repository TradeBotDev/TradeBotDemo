﻿using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        // Метод, показывающий главную страницу сайта.
        public async Task<IActionResult> Index()
        {
            Log.Information("HomeController: метод Index принял запрос GET.");

            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }
    }
}