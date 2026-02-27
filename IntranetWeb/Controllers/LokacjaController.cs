using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class LokacjaController : BaseSearchController<Lokacja>
    {
        private readonly ILokacjaService _lokacjaService;

        public LokacjaController(DataContext context, ILokacjaService lokacjaService) : base(context)
        {
            _lokacjaService = lokacjaService;
        }

        // GET: Lokacja
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _lokacjaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Lokacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailsData = await _lokacjaService.GetDetailsDataAsync(id.Value);
            if (detailsData == null)
            {
                return NotFound();
            }

            return View(detailsData);
        }

        // GET: Lokacja/Create
        public IActionResult Create()
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa");
            return View();
        }

        // POST: Lokacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLokacji,IdMagazynu,Kod,Opis,CzyAktywna")] Lokacja lokacja)
        {
            if (await CzyKodLokacjiIstniejeAsync(lokacja.IdMagazynu, lokacja.Kod))
            {
                ModelState.AddModelError(nameof(Lokacja.Kod), $"Lokacja o kodzie '{lokacja.Kod}' już istnieje w wybranym magazynie.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(lokacja);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Lokacja.Kod), $"Lokacja o kodzie '{lokacja.Kod}' już istnieje w wybranym magazynie.");
                }
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // GET: Lokacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacja = await _context.Lokacja.FindAsync(id);
            if (lokacja == null)
            {
                return NotFound();
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // POST: Lokacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLokacji,IdMagazynu,Kod,Opis,CzyAktywna")] Lokacja lokacja)
        {
            if (id != lokacja.IdLokacji)
            {
                return NotFound();
            }

            if (await CzyKodLokacjiIstniejeAsync(lokacja.IdMagazynu, lokacja.Kod, lokacja.IdLokacji))
            {
                ModelState.AddModelError(nameof(Lokacja.Kod), $"Lokacja o kodzie '{lokacja.Kod}' już istnieje w wybranym magazynie.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Lokacja
                        .FirstOrDefaultAsync(x => x.IdLokacji == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.IdMagazynu = lokacja.IdMagazynu;
                    existing.Kod = lokacja.Kod;
                    existing.Opis = lokacja.Opis;
                    existing.CzyAktywna = lokacja.CzyAktywna;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LokacjaExists(lokacja.IdLokacji))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(Lokacja.Kod), $"Lokacja o kodzie '{lokacja.Kod}' już istnieje w wybranym magazynie.");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Lokacja.Kod), $"Lokacja o kodzie '{lokacja.Kod}' już istnieje w wybranym magazynie.");
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["IdMagazynu"] = new SelectList(_context.Set<Magazyn>(), "IdMagazynu", "Nazwa", lokacja.IdMagazynu);
            return View(lokacja);
        }

        // GET: Lokacja/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacja = await _context.Lokacja
                .Include(l => l.Magazyn)
                .FirstOrDefaultAsync(m => m.IdLokacji == id);
            if (lokacja == null)
            {
                return NotFound();
            }

            await UzupelnijDaneUsuwaniaAsync(lokacja.IdLokacji);
            return View(lokacja);
        }

        // POST: Lokacja/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lokacja = await _context.Lokacja
                .Include(l => l.Magazyn)
                .FirstOrDefaultAsync(l => l.IdLokacji == id);
            if (lokacja == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var blockers = await PobierzBlokeryUsuwaniaAsync(id);
            if (blockers.MaPowiazania)
            {
                await UzupelnijDaneUsuwaniaAsync(id, blockers, "Nie można usunąć lokacji, ponieważ ma powiązane rekordy.");
                return View("Delete", lokacja);
            }

            try
            {
                _context.Lokacja.Remove(lokacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                blockers = await PobierzBlokeryUsuwaniaAsync(id);
                await UzupelnijDaneUsuwaniaAsync(id, blockers, "Nie można usunąć lokacji. Najpierw usuń lub przenieś powiązane rekordy.");
                return View("Delete", lokacja);
            }
        }

        private bool LokacjaExists(int id)
        {
            return _context.Lokacja.Any(e => e.IdLokacji == id);
        }

        private async Task<bool> CzyKodLokacjiIstniejeAsync(int idMagazynu, string? kod, int? excludeIdLokacji = null)
        {
            if (idMagazynu <= 0 || string.IsNullOrWhiteSpace(kod))
            {
                return false;
            }

            var normalizedKod = kod.Trim();
            return await _context.Lokacja.AnyAsync(l =>
                l.IdMagazynu == idMagazynu &&
                l.Kod == normalizedKod &&
                (!excludeIdLokacji.HasValue || l.IdLokacji != excludeIdLokacji.Value));
        }

        private async Task UzupelnijDaneUsuwaniaAsync(int idLokacji, LokacjaDeleteBlockers? blockers = null, string? errorMessage = null)
        {
            blockers ??= await PobierzBlokeryUsuwaniaAsync(idLokacji);

            ViewBag.LiczbaStanow = blockers.StanyMagazynowe;
            ViewBag.LiczbaPozycjiPz = blockers.PozycjePz;
            ViewBag.LiczbaPozycjiWz = blockers.PozycjeWz;
            ViewBag.LiczbaPozycjiMm = blockers.PozycjeMm;
            ViewBag.LiczbaRezerwacji = blockers.PozycjeRezerwacji;
            ViewBag.LiczbaPozycjiInwentaryzacji = blockers.PozycjeInwentaryzacji;
            ViewBag.LiczbaRuchow = blockers.RuchyMagazynowe;
            ViewBag.CzyMoznaUsunac = !blockers.MaPowiazania;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.DeleteErrorMessage = errorMessage;
            }
        }

        private async Task<LokacjaDeleteBlockers> PobierzBlokeryUsuwaniaAsync(int idLokacji)
        {
            var stanyMagazynowe = await _context.StanMagazynowy.CountAsync(s => s.IdLokacji == idLokacji);
            var pozycjePz = await _context.PozycjaPZ.CountAsync(p => p.IdLokacji == idLokacji);
            var pozycjeWz = await _context.PozycjaWZ.CountAsync(p => p.IdLokacji == idLokacji);
            var pozycjeMm = await _context.PozycjaMM.CountAsync(p => p.IdLokacjiZ == idLokacji || p.IdLokacjiDo == idLokacji);
            var pozycjeRezerwacji = await _context.PozycjaRezerwacji.CountAsync(p => p.IdLokacji == idLokacji);
            var pozycjeInwentaryzacji = await _context.PozycjaInwentaryzacji.CountAsync(p => p.IdLokacji == idLokacji);
            var ruchyMagazynowe = await _context.RuchMagazynowy.CountAsync(r => r.IdLokacjiZ == idLokacji || r.IdLokacjiDo == idLokacji);

            return new LokacjaDeleteBlockers
            {
                StanyMagazynowe = stanyMagazynowe,
                PozycjePz = pozycjePz,
                PozycjeWz = pozycjeWz,
                PozycjeMm = pozycjeMm,
                PozycjeRezerwacji = pozycjeRezerwacji,
                PozycjeInwentaryzacji = pozycjeInwentaryzacji,
                RuchyMagazynowe = ruchyMagazynowe
            };
        }

        private sealed class LokacjaDeleteBlockers
        {
            public int StanyMagazynowe { get; set; }
            public int PozycjePz { get; set; }
            public int PozycjeWz { get; set; }
            public int PozycjeMm { get; set; }
            public int PozycjeRezerwacji { get; set; }
            public int PozycjeInwentaryzacji { get; set; }
            public int RuchyMagazynowe { get; set; }

            public bool MaPowiazania =>
                StanyMagazynowe > 0 ||
                PozycjePz > 0 ||
                PozycjeWz > 0 ||
                PozycjeMm > 0 ||
                PozycjeRezerwacji > 0 ||
                PozycjeInwentaryzacji > 0 ||
                RuchyMagazynowe > 0;
        }
    }
}