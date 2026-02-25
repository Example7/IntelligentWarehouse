
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class KategoriaController : BaseSearchController<Kategoria>
    {
        private readonly IKategoriaService _kategoriaService;

        public KategoriaController(DataContext context, IKategoriaService kategoriaService) : base(context)
        {
            _kategoriaService = kategoriaService;
        }

        // GET: Kategoria
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Kategoria.Include(k => k.NadrzednaKategoria).AsNoTracking();
            query = ApplySearchAny(query, searchTerm, x => x.Nazwa, x => x.Sciezka);

            var kategorie = await query.ToListAsync();
            await UstawLicznikiKategoriiAsync(kategorie.Select(x => x.IdKategorii));
            return View(kategorie);
        }

        // GET: Kategoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailsData = await _kategoriaService.GetDetailsDataAsync(id.Value);
            if (detailsData == null)
            {
                return NotFound();
            }

            return View(detailsData);
        }

        // GET: Kategoria/Create
        public IActionResult Create()
        {
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa");
            return View();
        }

        // POST: Kategoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKategorii,IdNadrzednejKategorii,Nazwa")] Kategoria kategoria)
        {
            if (kategoria.IdNadrzednejKategorii.HasValue && !await KategoriaExistsAsync(kategoria.IdNadrzednejKategorii.Value))
            {
                ModelState.AddModelError(nameof(Kategoria.IdNadrzednejKategorii), "Wybrana kategoria nadrzedna nie istnieje.");
            }

            if (ModelState.IsValid)
            {
                kategoria.Sciezka = await ZbudujSciezkeKategoriiAsync(kategoria.Nazwa, kategoria.IdNadrzednejKategorii);
                _context.Add(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria, "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // GET: Kategoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoria.FindAsync(id);
            if (kategoria == null)
            {
                return NotFound();
            }
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria.Where(x => x.IdKategorii != kategoria.IdKategorii), "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // POST: Kategoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKategorii,IdNadrzednejKategorii,Nazwa")] Kategoria kategoria)
        {
            if (id != kategoria.IdKategorii)
            {
                return NotFound();
            }

            if (kategoria.IdNadrzednejKategorii == kategoria.IdKategorii)
            {
                ModelState.AddModelError(nameof(Kategoria.IdNadrzednejKategorii), "Kategoria nie moze byc nadrzedna dla samej siebie.");
            }

            if (kategoria.IdNadrzednejKategorii.HasValue && !await KategoriaExistsAsync(kategoria.IdNadrzednejKategorii.Value))
            {
                ModelState.AddModelError(nameof(Kategoria.IdNadrzednejKategorii), "Wybrana kategoria nadrzedna nie istnieje.");
            }

            if (kategoria.IdNadrzednejKategorii.HasValue && await PowodujeCyklAsync(kategoria.IdKategorii, kategoria.IdNadrzednejKategorii))
            {
                ModelState.AddModelError(nameof(Kategoria.IdNadrzednejKategorii), "Nie mozna ustawic podkategorii jako kategorii nadrzednej.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kategoriaDb = await _context.Kategoria.FirstOrDefaultAsync(x => x.IdKategorii == id);
                    if (kategoriaDb == null)
                    {
                        return NotFound();
                    }

                    kategoriaDb.IdNadrzednejKategorii = kategoria.IdNadrzednejKategorii;
                    kategoriaDb.Nazwa = kategoria.Nazwa;
                    kategoriaDb.Sciezka = await ZbudujSciezkeKategoriiAsync(kategoriaDb.Nazwa, kategoriaDb.IdNadrzednejKategorii);

                    await AktualizujSciezkiPodkategoriiAsync(kategoriaDb.IdKategorii, kategoriaDb.Sciezka);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriaExists(kategoria.IdKategorii))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdNadrzednejKategorii"] = new SelectList(_context.Kategoria.Where(x => x.IdKategorii != kategoria.IdKategorii), "IdKategorii", "Nazwa", kategoria.IdNadrzednejKategorii);
            return View(kategoria);
        }

        // GET: Kategoria/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deleteData = await _kategoriaService.GetDeleteDataAsync(id.Value);
            if (deleteData == null)
            {
                return NotFound();
            }

            return View(deleteData);
        }

        // POST: Kategoria/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategoria = await _context.Kategoria
                .Include(k => k.NadrzednaKategoria)
                .FirstOrDefaultAsync(k => k.IdKategorii == id);

            if (kategoria == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var (liczbaProduktow, liczbaPodkategorii) = await _kategoriaService.GetDeleteBlockersAsync(id);
            if (liczbaProduktow > 0 || liczbaPodkategorii > 0)
            {
                var deleteData = await _kategoriaService.GetDeleteDataAsync(kategoria.IdKategorii);
                if (deleteData == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "Nie mozna usunac kategorii, poniewaz ma przypisane produkty lub podkategorie.");
                return View("Delete", deleteData);
            }

            try
            {
                _context.Kategoria.Remove(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var deleteData = await _kategoriaService.GetDeleteDataAsync(kategoria.IdKategorii);
                if (deleteData == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Nie udalo sie usunac kategorii z powodu istniejacych powiazan.");
                return View("Delete", deleteData);
            }
        }

        private bool KategoriaExists(int id)
        {
            return _context.Kategoria.Any(e => e.IdKategorii == id);
        }

        private async Task<bool> KategoriaExistsAsync(int id)
        {
            return await _context.Kategoria.AnyAsync(e => e.IdKategorii == id);
        }

        private async Task<string> ZbudujSciezkeKategoriiAsync(string nazwa, int? idNadrzednejKategorii)
        {
            var nazwaTrim = nazwa.Trim();
            if (!idNadrzednejKategorii.HasValue)
            {
                return nazwaTrim;
            }

            var nadrzedna = await _context.Kategoria
                .AsNoTracking()
                .Where(x => x.IdKategorii == idNadrzednejKategorii.Value)
                .Select(x => new { x.Sciezka, x.Nazwa })
                .FirstAsync();

            var sciezkaNadrzednej = string.IsNullOrWhiteSpace(nadrzedna.Sciezka) ? nadrzedna.Nazwa : nadrzedna.Sciezka;
            return $"{sciezkaNadrzednej}/{nazwaTrim}";
        }

        private async Task AktualizujSciezkiPodkategoriiAsync(int idKategorii, string sciezkaKategorii)
        {
            var podkategorie = await _context.Kategoria
                .Where(x => x.IdNadrzednejKategorii == idKategorii)
                .ToListAsync();

            foreach (var podkategoria in podkategorie)
            {
                podkategoria.Sciezka = $"{sciezkaKategorii}/{podkategoria.Nazwa.Trim()}";
                await AktualizujSciezkiPodkategoriiAsync(podkategoria.IdKategorii, podkategoria.Sciezka);
            }
        }

        private async Task<bool> PowodujeCyklAsync(int idKategorii, int? idNowejNadrzednej)
        {
            var aktualnyParentId = idNowejNadrzednej;

            while (aktualnyParentId.HasValue)
            {
                if (aktualnyParentId.Value == idKategorii)
                {
                    return true;
                }

                aktualnyParentId = await _context.Kategoria
                    .Where(x => x.IdKategorii == aktualnyParentId.Value)
                    .Select(x => x.IdNadrzednejKategorii)
                    .FirstOrDefaultAsync();
            }

            return false;
        }

        private async Task UstawLicznikiKategoriiAsync(IEnumerable<int> idsKategorii)
        {
            var ids = idsKategorii.Distinct().ToList();
            if (ids.Count == 0)
            {
                ViewBag.LiczbyProduktow = new Dictionary<int, int>();
                ViewBag.LiczbyPodkategorii = new Dictionary<int, int>();
                return;
            }

            var liczbyProduktow = await _context.Produkt
                .Where(p => ids.Contains(p.IdKategorii))
                .GroupBy(p => p.IdKategorii)
                .Select(g => new { IdKategorii = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdKategorii, x => x.Count);

            var liczbyPodkategorii = await _context.Kategoria
                .Where(k => k.IdNadrzednejKategorii.HasValue && ids.Contains(k.IdNadrzednejKategorii.Value))
                .GroupBy(k => k.IdNadrzednejKategorii!.Value)
                .Select(g => new { IdKategorii = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdKategorii, x => x.Count);

            ViewBag.LiczbyProduktow = liczbyProduktow;
            ViewBag.LiczbyPodkategorii = liczbyPodkategorii;
        }
    }
}


