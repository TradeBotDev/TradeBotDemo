using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        private ILogger logger;

        // Добавление Id сессии в конструктор логгера.
        public HomeController()
            => logger = Log.ForContext<HomeController>()
            .ForContext("Where", "Website");

        // Метод, показывающий главную страницу сайта.
        public async Task<IActionResult> Index()
        {
            logger = logger.ForContext("SessionId", User.Identity.Name)
                .ForContext("Method", nameof(Index));
            logger.Information("{@Controller}: метод {@Method} принял запрос GET.", GetType().Name, "Index");

            // Проверка лицензии и передача ее результата в представление через ViewBag.
            var haveLicense = await Clients.LicenseClient.CheckLicense(User.Identity.Name, ProductCode.Tradebot);
            ViewBag.HaveLicense = haveLicense.HaveAccess;
            return View();
        }
    }
}