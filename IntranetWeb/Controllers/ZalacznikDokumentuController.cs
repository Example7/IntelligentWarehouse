
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using System.Text.Json;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
    public class ZalacznikDokumentuController : BaseSearchController<ZalacznikDokumentu>
    {
        private readonly IZalacznikDokumentuService _zalacznikDokumentuService;
        private readonly IWebHostEnvironment _environment;

        public ZalacznikDokumentuController(
            DataContext context,
            IZalacznikDokumentuService zalacznikDokumentuService,
            IWebHostEnvironment environment) : base(context)
        {
            _zalacznikDokumentuService = zalacznikDokumentuService;
            _environment = environment;
        }

        // GET: ZalacznikDokumentu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            return View(await _zalacznikDokumentuService.GetIndexDataAsync(searchTerm));
        }

        // GET: ZalacznikDokumentu/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _zalacznikDokumentuService.GetDetailsDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        // GET: ZalacznikDokumentu/Create
        public IActionResult Create()
        {
            PopulatePowiazaneDokumentySelectData();
            return View(new ZalacznikDokumentu
            {
                TypDokumentu = "PZ",
                NazwaPliku = string.Empty,
                ContentType = string.Empty,
                Sciezka = string.Empty
            });
        }

        // POST: ZalacznikDokumentu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka")] ZalacznikDokumentu zalacznikDokumentu, IFormFile? attachmentFile)
        {
            Normalize(zalacznikDokumentu);
            zalacznikDokumentu.WgranoUtc = DateTime.UtcNow;
            zalacznikDokumentu.WgralUserId = GetCurrentUserId();
            ModelState.Remove(nameof(ZalacznikDokumentu.NazwaPliku));
            ModelState.Remove(nameof(ZalacznikDokumentu.ContentType));
            ModelState.Remove(nameof(ZalacznikDokumentu.Sciezka));
            await TryStoreUploadedAttachmentAsync(zalacznikDokumentu, attachmentFile);

            if (attachmentFile == null || attachmentFile.Length <= 0)
            {
                ModelState.AddModelError(nameof(ZalacznikDokumentu.Sciezka), "Wybierz plik do wgrania.");
            }

            TryValidateModel(zalacznikDokumentu);

            if (!ModelState.IsValid)
            {
                PopulatePowiazaneDokumentySelectData();
                return View(zalacznikDokumentu);
            }

            _context.Add(zalacznikDokumentu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ZalacznikDokumentu/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zalacznikDokumentu = await _context.ZalacznikDokumentu.FindAsync(id);
            if (zalacznikDokumentu == null)
            {
                return NotFound();
            }
            PopulatePowiazaneDokumentySelectData();
            return View(zalacznikDokumentu);
        }

        // POST: ZalacznikDokumentu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka")] ZalacznikDokumentu zalacznikDokumentu, IFormFile? attachmentFile)
        {
            if (id != zalacznikDokumentu.Id)
            {
                return NotFound();
            }

            var existing = await _context.ZalacznikDokumentu.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            Normalize(zalacznikDokumentu);
            ModelState.Remove(nameof(ZalacznikDokumentu.NazwaPliku));
            ModelState.Remove(nameof(ZalacznikDokumentu.ContentType));
            ModelState.Remove(nameof(ZalacznikDokumentu.Sciezka));

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                await TryStoreUploadedAttachmentAsync(zalacznikDokumentu, attachmentFile);
                TryValidateModel(zalacznikDokumentu);
            }
            else
            {
                zalacznikDokumentu.NazwaPliku = existing.NazwaPliku;
                zalacznikDokumentu.ContentType = existing.ContentType;
                zalacznikDokumentu.Sciezka = existing.Sciezka;
                TryValidateModel(zalacznikDokumentu);
            }

            if (!ModelState.IsValid)
            {
                PopulatePowiazaneDokumentySelectData();
                return View(zalacznikDokumentu);
            }

            existing.TypDokumentu = zalacznikDokumentu.TypDokumentu;
            existing.IdDokumentu = zalacznikDokumentu.IdDokumentu;
            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                existing.NazwaPliku = zalacznikDokumentu.NazwaPliku;
                existing.ContentType = zalacznikDokumentu.ContentType;
                existing.Sciezka = zalacznikDokumentu.Sciezka;
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
                if (!ZalacznikDokumentuExists(zalacznikDokumentu.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // GET: ZalacznikDokumentu/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dto = await _zalacznikDokumentuService.GetDeleteDataAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            return View(dto);
        }

        // POST: ZalacznikDokumentu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var zalacznikDokumentu = await _context.ZalacznikDokumentu.FindAsync(id);
            if (zalacznikDokumentu != null)
            {
                _context.ZalacznikDokumentu.Remove(zalacznikDokumentu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZalacznikDokumentuExists(long id)
        {
            return _context.ZalacznikDokumentu.Any(e => e.Id == id);
        }

        private void PopulatePowiazaneDokumentySelectData()
        {
            var pz = _context.DokumentPZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .OrderByDescending(x => x.Id)
                .Take(200)
                .Select(x => new
                {
                    Typ = "PZ",
                    x.Id,
                    Label = $"{x.Numer} | {x.Status} | {x.Magazyn.Nazwa}"
                })
                .ToList();

            var wz = _context.DokumentWZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .OrderByDescending(x => x.Id)
                .Take(200)
                .Select(x => new
                {
                    Typ = "WZ",
                    x.Id,
                    Label = $"{x.Numer} | {x.Status} | {x.Magazyn.Nazwa}"
                })
                .ToList();

            var mm = _context.DokumentMM
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .OrderByDescending(x => x.Id)
                .Take(200)
                .Select(x => new
                {
                    Typ = "MM",
                    x.Id,
                    Label = $"{x.Numer} | {x.Status} | {x.Magazyn.Nazwa}"
                })
                .ToList();

            var all = pz.Cast<object>().Concat(wz).Concat(mm).ToList();
            ViewData["PowiazaneDokumentyJson"] = JsonSerializer.Serialize(all);
        }

        private static void Normalize(ZalacznikDokumentu zalacznikDokumentu)
        {
            zalacznikDokumentu.TypDokumentu = (zalacznikDokumentu.TypDokumentu ?? string.Empty).Trim().ToUpperInvariant();
            zalacznikDokumentu.NazwaPliku = (zalacznikDokumentu.NazwaPliku ?? string.Empty).Trim();
            zalacznikDokumentu.ContentType = (zalacznikDokumentu.ContentType ?? string.Empty).Trim();
            zalacznikDokumentu.Sciezka = (zalacznikDokumentu.Sciezka ?? string.Empty).Trim();
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var userId) ? userId : null;
        }

        private async Task TryStoreUploadedAttachmentAsync(ZalacznikDokumentu model, IFormFile? attachmentFile)
        {
            if (attachmentFile == null || attachmentFile.Length <= 0)
            {
                return;
            }

            var safeOriginalName = Path.GetFileName(attachmentFile.FileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(safeOriginalName))
            {
                ModelState.AddModelError(nameof(ZalacznikDokumentu.NazwaPliku), "Nieprawidłowa nazwa pliku.");
                return;
            }

            var ext = Path.GetExtension(safeOriginalName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".png", ".jpg", ".jpeg", ".zip"
            };

            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(ZalacznikDokumentu.NazwaPliku), "Dozwolone rozszerzenia: .pdf, .doc, .docx, .xls, .xlsx, .csv, .txt, .png, .jpg, .jpeg, .zip.");
                return;
            }

            var year = DateTime.UtcNow.ToString("yyyy");
            var month = DateTime.UtcNow.ToString("MM");
            var docTypeFolder = NormalizeTypeFolder(model.TypDokumentu);
            var baseName = Path.GetFileNameWithoutExtension(safeOriginalName);
            var safeBase = string.Concat(baseName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');
            if (string.IsNullOrWhiteSpace(safeBase))
            {
                safeBase = "zalacznik";
            }

            var finalFileName = $"{safeBase}_{Guid.NewGuid():N}{ext}";
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var targetDirectory = Path.Combine(webRoot, "uploads", "attachments", docTypeFolder, year, month);
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, finalFileName);
            await using (var fileStream = new FileStream(targetPath, FileMode.Create))
            {
                await attachmentFile.CopyToAsync(fileStream);
            }

            model.NazwaPliku = safeOriginalName;
            model.ContentType = string.IsNullOrWhiteSpace(attachmentFile.ContentType) ? "application/octet-stream" : attachmentFile.ContentType.Trim();
            model.Sciezka = $"/uploads/attachments/{docTypeFolder}/{year}/{month}/{finalFileName}";
        }

        private static string NormalizeTypeFolder(string? typDokumentu)
        {
            var normalized = (typDokumentu ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "pz" => "pz",
                "wz" => "wz",
                "mm" => "mm",
                _ => string.IsNullOrWhiteSpace(normalized) ? "inne" : normalized
            };
        }
    }
}


