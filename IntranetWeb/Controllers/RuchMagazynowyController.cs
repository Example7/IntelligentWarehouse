using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class RuchMagazynowyController : BaseSearchController<RuchMagazynowy>
    {
        private readonly IRuchMagazynowyService _ruchMagazynowyService;

        public RuchMagazynowyController(DataContext context, IRuchMagazynowyService ruchMagazynowyService) : base(context)
        {
            _ruchMagazynowyService = ruchMagazynowyService;
        }

        // GET: RuchMagazynowy
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _ruchMagazynowyService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: RuchMagazynowy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                .Include(r => r.LokacjaZ)
                .Include(r => r.Produkt)
                .Include(r => r.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdRuchu == id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            // Defensive explicit loads for nested refs used in the view.
            // In some environments nested navs may still arrive null despite Include/ThenInclude.
            if (ruchMagazynowy.Produkt != null)
            {
                await _context.Entry(ruchMagazynowy.Produkt).Reference(p => p.DomyslnaJednostka).LoadAsync();
            }

            if (ruchMagazynowy.LokacjaZ != null)
            {
                await _context.Entry(ruchMagazynowy.LokacjaZ).Reference(l => l.Magazyn).LoadAsync();
            }

            if (ruchMagazynowy.LokacjaDo != null)
            {
                await _context.Entry(ruchMagazynowy.LokacjaDo).Reference(l => l.Magazyn).LoadAsync();
            }

            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Create
        public IActionResult Create()
        {
            UzupelnijDaneFormularza();
            return View();
        }

        // POST: RuchMagazynowy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRuchu,Typ,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc,Referencja,Notatka")] RuchMagazynowy ruchMagazynowy)
        {
            ruchMagazynowy.IdUzytkownika = TryGetCurrentUserId();

            if (ModelState.IsValid)
            {
                var result = await _ruchMagazynowyService.CreateAndApplyAsync(ruchMagazynowy);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Message);
                }
            }

            UzupelnijDaneFormularza(ruchMagazynowy.IdProduktu, ruchMagazynowy.IdLokacjiZ, ruchMagazynowy.IdLokacjiDo, ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy.FindAsync(id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            UzupelnijDaneFormularza(ruchMagazynowy.IdProduktu, ruchMagazynowy.IdLokacjiZ, ruchMagazynowy.IdLokacjiDo, ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // POST: RuchMagazynowy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRuchu,Typ,IdProduktu,IdLokacjiZ,IdLokacjiDo,Ilosc,Referencja,Notatka,IdUzytkownika")] RuchMagazynowy ruchMagazynowy)
        {
            if (id != ruchMagazynowy.IdRuchu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _ruchMagazynowyService.UpdateAndReapplyAsync(id, ruchMagazynowy);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Message);
                }
            }

            UzupelnijDaneFormularza(ruchMagazynowy.IdProduktu, ruchMagazynowy.IdLokacjiZ, ruchMagazynowy.IdLokacjiDo, ruchMagazynowy.IdUzytkownika);
            return View(ruchMagazynowy);
        }

        // GET: RuchMagazynowy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ruchMagazynowy = await _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                .Include(r => r.LokacjaZ)
                .Include(r => r.Produkt)
                .Include(r => r.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdRuchu == id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            if (ruchMagazynowy.Produkt != null)
            {
                await _context.Entry(ruchMagazynowy.Produkt).Reference(p => p.DomyslnaJednostka).LoadAsync();
            }

            if (ruchMagazynowy.LokacjaZ != null)
            {
                await _context.Entry(ruchMagazynowy.LokacjaZ).Reference(l => l.Magazyn).LoadAsync();
            }

            if (ruchMagazynowy.LokacjaDo != null)
            {
                await _context.Entry(ruchMagazynowy.LokacjaDo).Reference(l => l.Magazyn).LoadAsync();
            }

            return View(ruchMagazynowy);
        }

        // POST: RuchMagazynowy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _ruchMagazynowyService.DeleteAndRevertAsync(id);
            if (result.Success)
            {
                return RedirectToAction(nameof(Index));
            }

            var ruchMagazynowy = await _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                    .ThenInclude(l => l!.Magazyn)
                .Include(r => r.LokacjaZ)
                    .ThenInclude(l => l!.Magazyn)
                .Include(r => r.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(r => r.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdRuchu == id);
            if (ruchMagazynowy == null)
            {
                return NotFound();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key, error.Message);
            }

            return View("Delete", ruchMagazynowy);
        }

        private bool RuchMagazynowyExists(int id)
        {
            return _context.RuchMagazynowy.Any(e => e.IdRuchu == id);
        }

        private void UzupelnijDaneFormularza(int? selectedProduktId = null, int? selectedLokacjaZId = null, int? selectedLokacjaDoId = null, int? selectedUzytkownikId = null)
        {
            var produkty = _context.Produkt
                .AsNoTracking()
                .Include(p => p.DomyslnaJednostka)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .Select(p => new
                {
                    p.IdProduktu,
                    Text = p.Kod + " - " + p.Nazwa + " (" + (p.DomyslnaJednostka != null ? p.DomyslnaJednostka.Kod : "j.m.") + ")"
                })
                .ToList();

            var lokacje = _context.Lokacja
                .AsNoTracking()
                .Include(l => l.Magazyn)
                .OrderBy(l => l.Magazyn.Nazwa)
                .ThenBy(l => l.Kod)
                .Select(l => new
                {
                    l.IdLokacji,
                    Text = (l.Magazyn != null ? l.Magazyn.Nazwa : "-") + " / " + l.Kod
                })
                .ToList();

            ViewData["IdProduktu"] = new SelectList(produkty, "IdProduktu", "Text", selectedProduktId);
            ViewData["IdLokacjiZ"] = new SelectList(lokacje, "IdLokacji", "Text", selectedLokacjaZId);
            ViewData["IdLokacjiDo"] = new SelectList(lokacje, "IdLokacji", "Text", selectedLokacjaDoId);
            ViewData["IdUzytkownika"] = new SelectList(_context.Set<Uzytkownik>().AsNoTracking().OrderBy(u => u.Email), "IdUzytkownika", "Email", selectedUzytkownikId);

            var pzReferences = _context.DokumentPZ
                .AsNoTracking()
                .Where(d => d.Status == "Posted")
                .OrderByDescending(d => d.ZaksiegowanoUtc ?? d.DataPrzyjeciaUtc)
                .ThenByDescending(d => d.Id)
                .Select(d => d.Numer)
                .Take(200)
                .ToList();

            var wzReferences = _context.DokumentWZ
                .AsNoTracking()
                .Where(d => d.Status == "Posted")
                .OrderByDescending(d => d.ZaksiegowanoUtc ?? d.DataWydaniaUtc)
                .ThenByDescending(d => d.Id)
                .Select(d => d.Numer)
                .Take(200)
                .ToList();

            var mmReferences = _context.DokumentMM
                .AsNoTracking()
                .Where(d => d.Status == "Posted")
                .OrderByDescending(d => d.ZaksiegowanoUtc ?? d.DataUtc)
                .ThenByDescending(d => d.Id)
                .Select(d => d.Numer)
                .Take(200)
                .ToList();

            var inwentaryzacjaReferences = _context.Inwentaryzacja
                .AsNoTracking()
                .Where(d => d.Status == "Closed")
                .OrderByDescending(d => d.KoniecUtc ?? d.StartUtc)
                .ThenByDescending(d => d.Id)
                .Select(d => d.Numer)
                .Take(200)
                .ToList();

            var referenceOptionsByType = new Dictionary<string, List<string>>
            {
                [((int)TypRuchuMagazynowego.Przyjęcie).ToString()] = pzReferences,
                [((int)TypRuchuMagazynowego.Wydanie).ToString()] = wzReferences,
                [((int)TypRuchuMagazynowego.Przesunięcie).ToString()] = mmReferences,
                [((int)TypRuchuMagazynowego.Korekta).ToString()] = new List<string>(),
                [((int)TypRuchuMagazynowego.Inwentaryzacja).ToString()] = inwentaryzacjaReferences
            };

            ViewData["ReferenceOptionsByTypeJson"] = JsonSerializer.Serialize(referenceOptionsByType);
        }

        private int? TryGetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
