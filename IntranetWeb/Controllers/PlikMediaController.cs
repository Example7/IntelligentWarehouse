using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class PlikMediaController : BaseSearchController<PlikMedia>
    {
        private readonly IPlikMediaService _plikMediaService;
        private readonly IWebHostEnvironment _environment;

        public PlikMediaController(
            DataContext context,
            IPlikMediaService plikMediaService,
            IWebHostEnvironment environment) : base(context)
        {
            _plikMediaService = plikMediaService;
            _environment = environment;
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
            return View(new PlikMedia
            {
                NazwaPliku = string.Empty,
                ContentType = string.Empty,
                Sciezka = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis")] PlikMedia plikMedia, IFormFile? mediaFile)
        {
            Normalize(plikMedia);
            plikMedia.WgranoUtc = DateTime.UtcNow;
            plikMedia.WgralUserId = GetCurrentUserId();
            ModelState.Remove(nameof(PlikMedia.NazwaPliku));
            ModelState.Remove(nameof(PlikMedia.ContentType));
            ModelState.Remove(nameof(PlikMedia.Sciezka));
            ModelState.Remove(nameof(PlikMedia.RozmiarBajty));
            await TryStoreUploadedMediaAsync(plikMedia, mediaFile);

            if (mediaFile == null || mediaFile.Length <= 0)
            {
                ModelState.AddModelError(nameof(PlikMedia.Sciezka), "Wybierz plik do wgrania.");
            }

            TryValidateModel(plikMedia);

            if (!ModelState.IsValid)
            {
                return View(plikMedia);
            }

            try
            {
                _context.Add(plikMedia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(PlikMedia.Sciezka), "Plik o takiej ścieżce już istnieje.");
                return View(plikMedia);
            }
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

            return View(plikMedia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,NazwaPliku,ContentType,Sciezka,RozmiarBajty,Opis")] PlikMedia plikMedia, IFormFile? mediaFile)
        {
            if (id != plikMedia.Id)
            {
                return NotFound();
            }

            Normalize(plikMedia);
            if (mediaFile != null && mediaFile.Length > 0)
            {
                ModelState.Remove(nameof(PlikMedia.NazwaPliku));
                ModelState.Remove(nameof(PlikMedia.ContentType));
                ModelState.Remove(nameof(PlikMedia.Sciezka));
                ModelState.Remove(nameof(PlikMedia.RozmiarBajty));
            }
            await TryStoreUploadedMediaAsync(plikMedia, mediaFile);
            if (mediaFile != null && mediaFile.Length > 0)
            {
                TryValidateModel(plikMedia);
            }

            if (!ModelState.IsValid)
            {
                return View(plikMedia);
            }

            var existing = await _context.PlikMedia.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Opis = plikMedia.Opis;
            if (mediaFile != null && mediaFile.Length > 0)
            {
                existing.NazwaPliku = plikMedia.NazwaPliku;
                existing.ContentType = plikMedia.ContentType;
                existing.Sciezka = plikMedia.Sciezka;
                existing.RozmiarBajty = plikMedia.RozmiarBajty;
                existing.WgranoUtc = DateTime.UtcNow;
                existing.WgralUserId = GetCurrentUserId();
            }

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
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(PlikMedia.Sciezka), "Plik o takiej ścieżce już istnieje.");
                return View(plikMedia);
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

        private static void Normalize(PlikMedia plikMedia)
        {
            plikMedia.NazwaPliku = (plikMedia.NazwaPliku ?? string.Empty).Trim();
            plikMedia.ContentType = (plikMedia.ContentType ?? string.Empty).Trim();
            plikMedia.Sciezka = (plikMedia.Sciezka ?? string.Empty).Trim();
            plikMedia.Opis = string.IsNullOrWhiteSpace(plikMedia.Opis) ? null : plikMedia.Opis.Trim();
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var userId) ? userId : null;
        }

        private async Task TryStoreUploadedMediaAsync(PlikMedia model, IFormFile? mediaFile)
        {
            if (mediaFile == null || mediaFile.Length <= 0)
            {
                return;
            }

            var safeOriginalName = Path.GetFileName(mediaFile.FileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(safeOriginalName))
            {
                ModelState.AddModelError(nameof(PlikMedia.NazwaPliku), "Nieprawidłowa nazwa pliku.");
                return;
            }

            var extension = Path.GetExtension(safeOriginalName);
            var baseName = Path.GetFileNameWithoutExtension(safeOriginalName);
            var safeBaseName = string.Concat(baseName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');
            if (string.IsNullOrWhiteSpace(safeBaseName))
            {
                safeBaseName = "plik";
            }

            var finalFileName = $"{safeBaseName}_{Guid.NewGuid():N}{extension}";
            var year = DateTime.UtcNow.ToString("yyyy");
            var month = DateTime.UtcNow.ToString("MM");

            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var targetDirectory = Path.Combine(webRoot, "uploads", "media", year, month);
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, finalFileName);
            await using (var fileStream = new FileStream(targetPath, FileMode.Create))
            {
                await mediaFile.CopyToAsync(fileStream);
            }

            model.NazwaPliku = safeOriginalName;
            model.ContentType = string.IsNullOrWhiteSpace(mediaFile.ContentType) ? "application/octet-stream" : mediaFile.ContentType.Trim();
            model.RozmiarBajty = mediaFile.Length;
            model.Sciezka = $"/uploads/media/{year}/{month}/{finalFileName}";
        }
    }
}

