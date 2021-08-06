using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Главная страница";
            ViewBag.SectionTitle = "Представляем нашего бота";
            return View();
        }

        [Route("About")]
        public IActionResult About()
        {
            ViewBag.Title = "О нас";
            ViewBag.SectionTitle = "О нас";
            return View();
        }
    }
}
