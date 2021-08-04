using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

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
    }
}
