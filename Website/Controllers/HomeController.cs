using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Главная страница";
            ViewBag.SectionTitle = "Главная страница";
            return View();
        }

        [Route("About")]
        public IActionResult About()
        {
            ViewBag.Title = "О нас";
            ViewBag.SectionTitle = "О нас";
            return View();
        }

        [Route("Tradebot")]
        public IActionResult Tradebot()
        {
            return View();
        }
    }
}
