using AdminWeb.Models;
using Interfaces.CMS;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAktualnoscService _aktualnoscService;
        private readonly IStronaService _stronaService;

        public HomeController(ILogger<HomeController> logger, IAktualnoscService aktualnoscService, IStronaService stronaService)
        {
            _logger = logger;
            _aktualnoscService = aktualnoscService;
            _stronaService = stronaService;
        }

        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.ModelStrony = await _stronaService.GetStronyByPosition();

            ViewBag.ModelAktualnosci = await _aktualnoscService.GetAktualnosciByPosition(3);

            if (id == null) id = 1;
            var strona = await _stronaService.GetStronaById(id);

            return View(strona);
        }

        public IActionResult OpisFirmy() { return View(); }

        public IActionResult HistoriaFirmy() { return View(); }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
