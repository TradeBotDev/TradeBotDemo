using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Controllers
{
    //[Authorize]
    public class PurchaseController : Controller
    {
        [HttpGet]
        public IActionResult Buy()
        {
            ViewBag.Title = "Оформление покупки";
            ViewBag.SectionTitle = "Оформление покупки";
            return View();
        }

        public IActionResult Buy(CreditCardModel model)
        {
            if (ModelState.IsValid) return Content($"{model.CardNumber}, {model.Date}, {model.CVV}");
            else return Content("Ошибка");
        }
    }
}
