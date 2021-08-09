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
    }
}
