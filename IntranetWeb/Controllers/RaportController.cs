using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Data.Magazyn;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class RaportController : Controller
    {
        private readonly IRaportMagazynowyService _raportMagazynowyService;

        public RaportController(IRaportMagazynowyService raportMagazynowyService)
        {
            _raportMagazynowyService = raportMagazynowyService;
        }

        public async Task<IActionResult> StanyMagazynowe(string? searchTerm, int? idMagazynu)
        {
            var model = await _raportMagazynowyService.GetRaportStanowAsync(searchTerm, idMagazynu);
            return View(model);
        }

        public async Task<IActionResult> RuchyMagazynowe(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var model = await _raportMagazynowyService.GetRaportRuchowAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            return View(model);
        }

        public async Task<IActionResult> EksportStanowExcel(string? searchTerm, int? idMagazynu)
        {
            var bytes = await _raportMagazynowyService.ExportRaportStanowExcelAsync(searchTerm, idMagazynu);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Raport_StanyMagazynowe_{date}.xlsx");
        }

        public async Task<IActionResult> EksportStanowPdf(string? searchTerm, int? idMagazynu)
        {
            var bytes = await _raportMagazynowyService.ExportRaportStanowPdfAsync(searchTerm, idMagazynu);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "application/pdf", $"Raport_StanyMagazynowe_{date}.pdf");
        }

        public async Task<IActionResult> EksportRuchowExcel(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var bytes = await _raportMagazynowyService.ExportRaportRuchowExcelAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Raport_RuchyMagazynowe_{date}.xlsx");
        }

        public async Task<IActionResult> EksportRuchowPdf(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var bytes = await _raportMagazynowyService.ExportRaportRuchowPdfAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "application/pdf", $"Raport_RuchyMagazynowe_{date}.pdf");
        }
    }
}
