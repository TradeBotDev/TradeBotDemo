using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        [HttpGet]
        public IActionResult Buy()
        {
            return View();
        }

        public IActionResult Buy(CreditCardModel model)
        {
            if (!ModelState.IsValid)
                return View();
            return Content($"{model.CardNumber}, {model.Date}, {model.CVV}");
        }
    }
}
