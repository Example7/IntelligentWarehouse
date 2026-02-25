using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;

namespace IntranetWeb.Controllers
{
    public class SzablonWydrukuController : BaseSearchController<SzablonWydruku>
    {
        private readonly ISzablonWydrukuService _szablonWydrukuService;

        public SzablonWydrukuController(DataContext context, ISzablonWydrukuService szablonWydrukuService) : base(context)
        {
            _szablonWydrukuService = szablonWydrukuService;
        }

        // GET: SzablonWydruku
        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _szablonWydrukuService.GetIndexDataAsync(searchTerm));
        }

        // GET: SzablonWydruku/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _szablonWydrukuService.GetDetailsDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        // GET: SzablonWydruku/Create
        public IActionResult Create()
        {
            PopulateUzytkownicySelect();
            return View(new SzablonWydruku
            {
                TypDokumentu = string.Empty,
                Nazwa = string.Empty,
                Wersja = "1.0",
                NazwaPliku = string.Empty,
                Sciezka = string.Empty
            });
        }

        // POST: SzablonWydruku/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgralUserId")] SzablonWydruku szablonWydruku)
        {
            Normalize(szablonWydruku);
            szablonWydruku.WgranoUtc = DateTime.UtcNow;

            await ValidateUniqueTypWersjaAsync(szablonWydruku);

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
                return View(szablonWydruku);
            }

            try
            {
                _context.Add(szablonWydruku);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Wersja), $"Szablon dla typu '{szablonWydruku.TypDokumentu}' i wersji '{szablonWydruku.Wersja}' już istnieje.");
                PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
                return View(szablonWydruku);
            }
        }

        // GET: SzablonWydruku/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szablonWydruku = await _context.SzablonWydruku.FindAsync(id);
            if (szablonWydruku == null)
            {
                return NotFound();
            }
            PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
            return View(szablonWydruku);
        }

        // POST: SzablonWydruku/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgralUserId")] SzablonWydruku szablonWydruku)
        {
            if (id != szablonWydruku.Id)
            {
                return NotFound();
            }

            Normalize(szablonWydruku);
            await ValidateUniqueTypWersjaAsync(szablonWydruku, szablonWydruku.Id);

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
                return View(szablonWydruku);
            }

            var existing = await _context.SzablonWydruku.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TypDokumentu = szablonWydruku.TypDokumentu;
            existing.Nazwa = szablonWydruku.Nazwa;
            existing.Wersja = szablonWydruku.Wersja;
            existing.NazwaPliku = szablonWydruku.NazwaPliku;
            existing.Sciezka = szablonWydruku.Sciezka;
            existing.CzyAktywny = szablonWydruku.CzyAktywny;
            existing.WgralUserId = szablonWydruku.WgralUserId;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SzablonWydrukuExists(szablonWydruku.Id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Wersja), $"Szablon dla typu '{szablonWydruku.TypDokumentu}' i wersji '{szablonWydruku.Wersja}' już istnieje.");
                PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
                return View(szablonWydruku);
            }
        }

        // GET: SzablonWydruku/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _szablonWydrukuService.GetDeleteDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        // POST: SzablonWydruku/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var szablonWydruku = await _context.SzablonWydruku.FindAsync(id);
            if (szablonWydruku != null)
            {
                _context.SzablonWydruku.Remove(szablonWydruku);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SzablonWydrukuExists(int id)
        {
            return _context.SzablonWydruku.Any(e => e.Id == id);
        }

        private void PopulateUzytkownicySelect(int? selected = null)
        {
            var users = _context.Uzytkownik
                .AsNoTracking()
                .OrderBy(x => x.Login)
                .Select(x => new
                {
                    x.IdUzytkownika,
                    Label = string.IsNullOrWhiteSpace(x.Email) ? x.Login : $"{x.Login} | {x.Email}"
                })
                .ToList();

            ViewData["WgralUserId"] = new SelectList(users, "IdUzytkownika", "Label", selected);
        }

        private static void Normalize(SzablonWydruku szablonWydruku)
        {
            szablonWydruku.TypDokumentu = (szablonWydruku.TypDokumentu ?? string.Empty).Trim();
            szablonWydruku.Nazwa = (szablonWydruku.Nazwa ?? string.Empty).Trim();
            szablonWydruku.Wersja = (szablonWydruku.Wersja ?? string.Empty).Trim();
            szablonWydruku.NazwaPliku = (szablonWydruku.NazwaPliku ?? string.Empty).Trim();
            szablonWydruku.Sciezka = (szablonWydruku.Sciezka ?? string.Empty).Trim();
        }

        private async Task ValidateUniqueTypWersjaAsync(SzablonWydruku szablonWydruku, int? excludeId = null)
        {
            if (await _szablonWydrukuService.TypIWersjaExistsAsync(szablonWydruku.TypDokumentu, szablonWydruku.Wersja, excludeId))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Wersja), $"Szablon dla typu '{szablonWydruku.TypDokumentu}' i wersji '{szablonWydruku.Wersja}' już istnieje.");
            }
        }
    }
}
