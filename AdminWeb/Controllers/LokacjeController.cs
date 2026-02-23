using Interfaces.Magazyn;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    public class LokacjeController : Controller
    {
        private readonly ILokacjaService _lokacjaService;

        public LokacjeController(ILokacjaService lokacjaService)
        {
            _lokacjaService = lokacjaService;
        }

        public async Task<IActionResult> Index()
        {
            var lokacje = await _lokacjaService.GetLokacje();
            return View(lokacje);
        }
    }
}