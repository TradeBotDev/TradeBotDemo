using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Controllers
{
    public class TradebotController : Controller
    {
        [Route("Tradebot")]
        public IActionResult Tradebot()
        {
            return View();
        }
    }
}
