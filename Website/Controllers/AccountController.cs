using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [Route("Account")]
        public IActionResult Account()
        {
            ViewBag.Title = "Аккаунт";
            ViewBag.SectionTitle = "Аккаунт";
            return View();
        }
    }
}
