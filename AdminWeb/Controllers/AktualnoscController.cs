using Interfaces.CMS;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    public class AktualnoscController : Controller
    {
        private readonly IAktualnoscService _aktualnoscService;
        public AktualnoscController(IAktualnoscService aktualnoscService)
        {
            _aktualnoscService = aktualnoscService;
        }

        public async Task<IActionResult> Index(int id)
        {
            ViewBag.ModelAktualnosci = await _aktualnoscService.GetAktualnosciByPosition(2);

            
            var aktualnosc = _aktualnoscService.GetAktualnoscById(id);
            if (aktualnosc == null) id = 1;
            return View(aktualnosc);
        }
    }
}
