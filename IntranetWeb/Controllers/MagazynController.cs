using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class MagazynController : BaseSearchController<Magazyn>
    {
        private readonly IMagazynService _magazynService;

        public MagazynController(DataContext context, IMagazynService magazynService) : base(context)
        {
            _magazynService = magazynService;
        }

        // GET: Magazyn
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _magazynService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Magazyn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailsData = await _magazynService.GetMagazynDetailsData(id.Value);
            if (detailsData == null)
            {
                return NotFound();
            }

            return View(detailsData);
        }

        // GET: Magazyn/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Magazyn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMagazynu,Nazwa,Adres,CzyAktywny")] Magazyn magazyn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(magazyn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(magazyn);
        }

        // GET: Magazyn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazyn = await _context.Magazyn.FindAsync(id);
            if (magazyn == null)
            {
                return NotFound();
            }
            return View(magazyn);
        }

        // POST: Magazyn/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMagazynu,Nazwa,Adres,CzyAktywny")] Magazyn magazyn)
        {
            if (id != magazyn.IdMagazynu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Magazyn
                        .FirstOrDefaultAsync(x => x.IdMagazynu == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Nazwa = magazyn.Nazwa;
                    existing.Adres = magazyn.Adres;
                    existing.CzyAktywny = magazyn.CzyAktywny;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MagazynExists(magazyn.IdMagazynu))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(magazyn);
        }

        // GET: Magazyn/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazyn = await _context.Magazyn
                .FirstOrDefaultAsync(m => m.IdMagazynu == id);
            if (magazyn == null)
            {
                return NotFound();
            }

            await UstawInformacjeODependencjachUsuwaniaAsync(magazyn.IdMagazynu);
            return View(magazyn);
        }

        // POST: Magazyn/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var magazyn = await _context.Magazyn.FindAsync(id);
            if (magazyn == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var blokady = await PobierzBlokadyUsuwaniaAsync(id);
            if (SumaBlokad(blokady) > 0)
            {
                UstawViewBagBlokad(blokady);
                ModelState.AddModelError(string.Empty, "Nie można usunąć magazynu, ponieważ ma powiązane rekordy.");
                return View("Delete", magazyn);
            }

            try
            {
                _context.Magazyn.Remove(magazyn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await UstawInformacjeODependencjachUsuwaniaAsync(id);
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć magazynu z powodu istniejących powiązań.");
                return View("Delete", magazyn);
            }
        }

        private bool MagazynExists(int id)
        {
            return _context.Magazyn.Any(e => e.IdMagazynu == id);
        }

        private sealed record BlokadyMagazynu(
            int Lokacje,
            int DokumentyPz,
            int DokumentyWz,
            int DokumentyMm,
            int Inwentaryzacje,
            int Rezerwacje,
            int RegulyAlertow,
            int Alerty);

        private async Task<BlokadyMagazynu> PobierzBlokadyUsuwaniaAsync(int idMagazynu)
        {
            var lokacje = await _context.Lokacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var dokumentyPz = await _context.DokumentPZ.CountAsync(x => x.IdMagazynu == idMagazynu);
            var dokumentyWz = await _context.DokumentWZ.CountAsync(x => x.IdMagazynu == idMagazynu);
            var dokumentyMm = await _context.DokumentMM.CountAsync(x => x.IdMagazynu == idMagazynu);
            var inwentaryzacje = await _context.Inwentaryzacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var rezerwacje = await _context.Rezerwacja.CountAsync(x => x.IdMagazynu == idMagazynu);
            var regulyAlertow = await _context.RegulaAlertu.CountAsync(x => x.IdMagazynu == idMagazynu);
            var alerty = await _context.Alert.CountAsync(x => x.IdMagazynu == idMagazynu);

            return new BlokadyMagazynu(
                lokacje,
                dokumentyPz,
                dokumentyWz,
                dokumentyMm,
                inwentaryzacje,
                rezerwacje,
                regulyAlertow,
                alerty);
        }

        private static int SumaBlokad(BlokadyMagazynu b) =>
            b.Lokacje + b.DokumentyPz + b.DokumentyWz + b.DokumentyMm + b.Inwentaryzacje + b.Rezerwacje + b.RegulyAlertow + b.Alerty;

        private async Task UstawInformacjeODependencjachUsuwaniaAsync(int idMagazynu)
        {
            var blokady = await PobierzBlokadyUsuwaniaAsync(idMagazynu);
            UstawViewBagBlokad(blokady);
        }

        private void UstawViewBagBlokad(BlokadyMagazynu blokady)
        {
            ViewBag.LiczbaLokacji = blokady.Lokacje;
            ViewBag.LiczbaDokumentowPz = blokady.DokumentyPz;
            ViewBag.LiczbaDokumentowWz = blokady.DokumentyWz;
            ViewBag.LiczbaDokumentowMm = blokady.DokumentyMm;
            ViewBag.LiczbaInwentaryzacji = blokady.Inwentaryzacje;
            ViewBag.LiczbaRezerwacji = blokady.Rezerwacje;
            ViewBag.LiczbaRegulAlertow = blokady.RegulyAlertow;
            ViewBag.LiczbaAlertow = blokady.Alerty;
            ViewBag.CzyMoznaUsunac = SumaBlokad(blokady) == 0;
        }
    }
}


