using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Controllers
{
    public class AccountController : Controller
    {
        [Route("Account")]
        public IActionResult Account()
        {
            return View();
        }
    }
}
