using Data.Data;
using Interfaces.Magazyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Controllers
{
    public class KategorieMenuComponent : ViewComponent
    {
        private readonly IKategoriaService _kategoriaService;

        public KategorieMenuComponent(IKategoriaService kategoriaService)
        {
            _kategoriaService = kategoriaService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("KategorieMenuComponent", await _kategoriaService.GetKategorie());
        }
    }
}
