using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Security;
using Interfaces.CMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class SzablonWydrukuController : BaseSearchController<SzablonWydruku>
    {
        private readonly ISzablonWydrukuService _szablonWydrukuService;
        private readonly IWebHostEnvironment _environment;

        public SzablonWydrukuController(
            DataContext context,
            ISzablonWydrukuService szablonWydrukuService,
            IWebHostEnvironment environment) : base(context)
        {
            _szablonWydrukuService = szablonWydrukuService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _szablonWydrukuService.GetIndexDataAsync(searchTerm));
        }

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

        public IActionResult Create()
        {
            return View(new SzablonWydruku
            {
                TypDokumentu = string.Empty,
                Nazwa = string.Empty,
                Wersja = "1.0",
                NazwaPliku = string.Empty,
                Sciezka = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,Nazwa,Wersja,CzyAktywny")] SzablonWydruku szablonWydruku, IFormFile? templateFile)
        {
            szablonWydruku.CzyAktywny = ResolveCheckboxValue(nameof(SzablonWydruku.CzyAktywny), szablonWydruku.CzyAktywny);
            Normalize(szablonWydruku);
            szablonWydruku.WgranoUtc = DateTime.UtcNow;
            szablonWydruku.WgralUserId = GetCurrentUserId();
            ModelState.Remove(nameof(SzablonWydruku.NazwaPliku));
            ModelState.Remove(nameof(SzablonWydruku.Sciezka));
            await TryStoreUploadedTemplateAsync(szablonWydruku, templateFile);

            await ValidateUniqueTypWersjaAsync(szablonWydruku);

            if (templateFile == null || templateFile.Length <= 0)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Sciezka), "Wybierz plik szablonu.");
            }

            TryValidateModel(szablonWydruku);

            if (!ModelState.IsValid)
            {
                return View(szablonWydruku);
            }

            try
            {
                if (szablonWydruku.CzyAktywny)
                {
                    await DeactivateOtherActiveTemplatesAsync(szablonWydruku.TypDokumentu);
                }

                _context.Add(szablonWydruku);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Wersja), $"Szablon dla typu '{szablonWydruku.TypDokumentu}' i wersji '{szablonWydruku.Wersja}' już istnieje.");
                return View(szablonWydruku);
            }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.NazwaPliku), "Nie udało się zapisać pliku szablonu. Upewnij się, że plik nie jest otwarty w innym programie.");
                return View(szablonWydruku);
            }
        }

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

            return View(szablonWydruku);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypDokumentu,Nazwa,Wersja,CzyAktywny")] SzablonWydruku szablonWydruku, IFormFile? templateFile)
        {
            if (id != szablonWydruku.Id)
            {
                return NotFound();
            }

            var existing = await _context.SzablonWydruku.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            szablonWydruku.CzyAktywny = ResolveCheckboxValue(nameof(SzablonWydruku.CzyAktywny), szablonWydruku.CzyAktywny);
            Normalize(szablonWydruku);
            ModelState.Remove(nameof(SzablonWydruku.NazwaPliku));
            ModelState.Remove(nameof(SzablonWydruku.Sciezka));

            if (templateFile != null && templateFile.Length > 0)
            {
                await TryStoreUploadedTemplateAsync(szablonWydruku, templateFile);
            }
            else
            {
                szablonWydruku.NazwaPliku = existing.NazwaPliku;
                szablonWydruku.Sciezka = existing.Sciezka;
            }

            await ValidateUniqueTypWersjaAsync(szablonWydruku, szablonWydruku.Id);
            TryValidateModel(szablonWydruku);

            if (!ModelState.IsValid)
            {
                return View(szablonWydruku);
            }

            existing.TypDokumentu = szablonWydruku.TypDokumentu;
            existing.Nazwa = szablonWydruku.Nazwa;
            existing.Wersja = szablonWydruku.Wersja;
            existing.CzyAktywny = szablonWydruku.CzyAktywny;
            if (templateFile != null && templateFile.Length > 0)
            {
                existing.NazwaPliku = szablonWydruku.NazwaPliku;
                existing.Sciezka = szablonWydruku.Sciezka;
                existing.WgranoUtc = DateTime.UtcNow;
                existing.WgralUserId = GetCurrentUserId();
            }

            try
            {
                if (existing.CzyAktywny)
                {
                    await DeactivateOtherActiveTemplatesAsync(existing.TypDokumentu, existing.Id);
                }

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
                return View(szablonWydruku);
            }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(SzablonWydruku.NazwaPliku), "Nie udało się zapisać pliku szablonu. Upewnij się, że plik nie jest otwarty w innym programie.");
                return View(szablonWydruku);
            }
        }

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

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var userId) ? userId : null;
        }

        private static void Normalize(SzablonWydruku szablonWydruku)
        {
            szablonWydruku.TypDokumentu = (szablonWydruku.TypDokumentu ?? string.Empty).Trim().ToUpperInvariant();
            szablonWydruku.Nazwa = (szablonWydruku.Nazwa ?? string.Empty).Trim();
            szablonWydruku.Wersja = (szablonWydruku.Wersja ?? string.Empty).Trim();
            szablonWydruku.NazwaPliku = (szablonWydruku.NazwaPliku ?? string.Empty).Trim();
            szablonWydruku.Sciezka = (szablonWydruku.Sciezka ?? string.Empty).Trim();
        }

        private bool ResolveCheckboxValue(string fieldName, bool fallback)
        {
            if (!Request.HasFormContentType || !Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
            {
                return fallback;
            }

            foreach (var value in values)
            {
                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
                    value == "1")
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ValidateUniqueTypWersjaAsync(SzablonWydruku szablonWydruku, int? excludeId = null)
        {
            if (await _szablonWydrukuService.TypIWersjaExistsAsync(szablonWydruku.TypDokumentu, szablonWydruku.Wersja, excludeId))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Wersja), $"Szablon dla typu '{szablonWydruku.TypDokumentu}' i wersji '{szablonWydruku.Wersja}' już istnieje.");
            }
        }

        private async Task TryStoreUploadedTemplateAsync(SzablonWydruku model, IFormFile? templateFile)
        {
            if (templateFile == null || templateFile.Length == 0)
            {
                return;
            }

            var ext = Path.GetExtension(templateFile.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".docx", ".html", ".htm", ".txt", ".md"
            };

            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.NazwaPliku), "Dozwolone rozszerzenia: .docx, .html, .htm, .txt, .md.");
                return;
            }

            var typeFolder = NormalizeTypeFolder(model.TypDokumentu);
            if (string.IsNullOrWhiteSpace(typeFolder))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.TypDokumentu), "Podaj typ dokumentu (np. WZ/PZ/MM) przed wgraniem pliku.");
                return;
            }

            var fileName = SanitizeFileName(Path.GetFileName(templateFile.FileName));
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.NazwaPliku), "Nieprawidłowa nazwa pliku.");
                return;
            }

            var targetDirectory = Path.Combine(_environment.ContentRootPath, "Templates", typeFolder);
            Directory.CreateDirectory(targetDirectory);

            var uniqueFileName = BuildUniqueFileName(targetDirectory, fileName);
            var targetPath = Path.Combine(targetDirectory, uniqueFileName);
            await using (var fileStream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                await templateFile.CopyToAsync(fileStream);
            }

            model.NazwaPliku = uniqueFileName;
            model.Sciezka = $"/templates/{typeFolder}/{uniqueFileName}";
        }

        private static string NormalizeTypeFolder(string? typDokumentu)
        {
            var normalized = (typDokumentu ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "wz" => "wz",
                "pz" => "pz",
                "mm" => "mm",
                _ => normalized
            };
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(fileName.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        }

        private static string BuildUniqueFileName(string directoryPath, string originalFileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);
            var ext = Path.GetExtension(originalFileName);
            var candidate = originalFileName;
            var counter = 1;

            while (System.IO.File.Exists(Path.Combine(directoryPath, candidate)))
            {
                candidate = $"{baseName}_{counter}{ext}";
                counter++;
            }

            return candidate;
        }

        private async Task DeactivateOtherActiveTemplatesAsync(string? typDokumentu, int? excludeId = null)
        {
            var normalizedType = (typDokumentu ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedType))
            {
                return;
            }

            var query = _context.SzablonWydruku
                .Where(x => x.CzyAktywny && x.TypDokumentu == normalizedType);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var activeTemplates = await query.ToListAsync();
            foreach (var template in activeTemplates)
            {
                template.CzyAktywny = false;
            }
        }
    }
}