using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using Microsoft.AspNetCore.Hosting;

namespace IntranetWeb.Controllers
{
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
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgralUserId")] SzablonWydruku szablonWydruku, IFormFile? templateFile)
        {
            Normalize(szablonWydruku);
            szablonWydruku.WgranoUtc = DateTime.UtcNow;
            await TryStoreUploadedTemplateAsync(szablonWydruku, templateFile);

            await ValidateUniqueTypWersjaAsync(szablonWydruku);

            if (templateFile == null && string.IsNullOrWhiteSpace(szablonWydruku.Sciezka))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Sciezka), "Podaj ścieżkę lub wgraj plik szablonu.");
            }

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(szablonWydruku.WgralUserId);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypDokumentu,Nazwa,Wersja,NazwaPliku,Sciezka,CzyAktywny,WgralUserId")] SzablonWydruku szablonWydruku, IFormFile? templateFile)
        {
            if (id != szablonWydruku.Id)
            {
                return NotFound();
            }

            Normalize(szablonWydruku);
            await TryStoreUploadedTemplateAsync(szablonWydruku, templateFile);
            await ValidateUniqueTypWersjaAsync(szablonWydruku, szablonWydruku.Id);

            if (templateFile == null && string.IsNullOrWhiteSpace(szablonWydruku.Sciezka))
            {
                ModelState.AddModelError(nameof(SzablonWydruku.Sciezka), "Podaj ścieżkę lub wgraj plik szablonu.");
            }

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
            if (templateFile != null && templateFile.Length > 0)
            {
                existing.WgranoUtc = DateTime.UtcNow;
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
                ModelState.AddModelError(nameof(SzablonWydruku.NazwaPliku), "Nieprawidlowa nazwa pliku.");
                return;
            }

            var targetDirectory = Path.Combine(_environment.ContentRootPath, "Templates", typeFolder);
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, fileName);
            await using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await templateFile.CopyToAsync(fileStream);
            }

            model.NazwaPliku = fileName;
            model.Sciezka = $"/templates/{typeFolder}/{fileName}";
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
