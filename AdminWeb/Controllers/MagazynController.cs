using Data.Data;
using Interfaces.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Controllers
{
    public class MagazynController : Controller
    {
        private readonly IProduktService _produktService;

        public MagazynController(IProduktService produktService)
        {
            _produktService = produktService;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) id = 1;
            var produktyDanegoRodzaju = await _produktService.GetProdukty(id.Value);

            return View(produktyDanegoRodzaju);
        }

        public async Task<IActionResult> Szczegoly(int id)
        {
            return View(await _produktService.GetProdukt(id));
        }
    }
}
    
