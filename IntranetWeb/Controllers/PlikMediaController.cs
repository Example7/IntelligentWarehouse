using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IntranetWeb.Controllers
{
    public class PlikMediaController : BaseSearchController<PlikMedia>
    {
        private readonly IPlikMediaService _plikMediaService;

        public PlikMediaController(DataContext context, IPlikMediaService plikMediaService) : base(context)
        {
            _plikMediaService = plikMediaService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _plikMediaService.GetIndexDataAsync(searchTerm));
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _plikMediaService.GetDetailsDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        public IActionResult Create()
        {
            PopulateUzytkownicySelect();
            return View(new PlikMedia
            {
                NazwaPliku = string.Empty,
                ContentType = string.Empty,
                Sciezka = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis,WgralUserId")] PlikMedia plikMedia)
        {
            Normalize(plikMedia);
            plikMedia.WgranoUtc = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(plikMedia.WgralUserId);
                return View(plikMedia);
            }

            _context.Add(plikMedia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plikMedia = await _context.PlikMedia.FindAsync(id);
            if (plikMedia == null)
            {
                return NotFound();
            }

            PopulateUzytkownicySelect(plikMedia.WgralUserId);
            return View(plikMedia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis,WgralUserId")] PlikMedia plikMedia)
        {
            if (id != plikMedia.Id)
            {
                return NotFound();
            }

            Normalize(plikMedia);

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(plikMedia.WgralUserId);
                return View(plikMedia);
            }

            var existing = await _context.PlikMedia.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.NazwaPliku = plikMedia.NazwaPliku;
            existing.ContentType = plikMedia.ContentType;
            existing.Sciezka = plikMedia.Sciezka;
            existing.RozmiarBajty = plikMedia.RozmiarBajty;
            existing.Opis = plikMedia.Opis;
            existing.WgralUserId = plikMedia.WgralUserId;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PlikMedia.AnyAsync(e => e.Id == plikMedia.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _plikMediaService.GetDeleteDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var plikMedia = await _context.PlikMedia.FindAsync(id);
            if (plikMedia != null)
            {
                _context.PlikMedia.Remove(plikMedia);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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

        private static void Normalize(PlikMedia plikMedia)
        {
            plikMedia.NazwaPliku = (plikMedia.NazwaPliku ?? string.Empty).Trim();
            plikMedia.ContentType = (plikMedia.ContentType ?? string.Empty).Trim();
            plikMedia.Sciezka = (plikMedia.Sciezka ?? string.Empty).Trim();
            plikMedia.Opis = string.IsNullOrWhiteSpace(plikMedia.Opis) ? null : plikMedia.Opis.Trim();
        }
    }
}
