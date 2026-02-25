using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class LogAudytuController : BaseSearchController<LogAudytu>
    {
        private readonly ILogAudytuService _logAudytuService;

        public LogAudytuController(DataContext context, ILogAudytuService logAudytuService) : base(context)
        {
            _logAudytuService = logAudytuService;
        }

        // GET: LogAudytu
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _logAudytuService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: LogAudytu/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _logAudytuService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: LogAudytu/Create
        public IActionResult Create()
        {
            var model = new LogAudytu { KiedyUtc = DateTime.UtcNow };
            UzupelnijDaneFormularza(model.UserId);
            return View(model);
        }

        // POST: LogAudytu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            NormalizujLog(logAudytu, normalizeLocalTimeToUtc: true);

            if (ModelState.IsValid)
            {
                _context.Add(logAudytu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            UzupelnijDaneFormularza(logAudytu.UserId);
            return View(logAudytu);
        }

        // GET: LogAudytu/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logAudytu = await _context.LogAudytu.FindAsync(id);
            if (logAudytu == null)
            {
                return NotFound();
            }

            // Formularz pokazuje czas lokalny.
            logAudytu.KiedyUtc = DateTime.SpecifyKind(logAudytu.KiedyUtc, DateTimeKind.Utc).ToLocalTime();
            UzupelnijDaneFormularza(logAudytu.UserId);
            return View(logAudytu);
        }

        // POST: LogAudytu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            if (id != logAudytu.Id)
            {
                return NotFound();
            }

            NormalizujLog(logAudytu, normalizeLocalTimeToUtc: true);

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.LogAudytu.FirstOrDefaultAsync(l => l.Id == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.UserId = logAudytu.UserId;
                    existing.Akcja = logAudytu.Akcja;
                    existing.Encja = logAudytu.Encja;
                    existing.IdEncji = logAudytu.IdEncji;
                    existing.KiedyUtc = logAudytu.KiedyUtc;
                    existing.StareJson = logAudytu.StareJson;
                    existing.NoweJson = logAudytu.NoweJson;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogAudytuExists(logAudytu.Id))
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
            UzupelnijDaneFormularza(logAudytu.UserId);
            return View(logAudytu);
        }

        // GET: LogAudytu/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _logAudytuService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: LogAudytu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var logAudytu = await _context.LogAudytu.FindAsync(id);
            if (logAudytu != null)
            {
                _context.LogAudytu.Remove(logAudytu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogAudytuExists(long id)
        {
            return _context.LogAudytu.Any(e => e.Id == id);
        }

        private void UzupelnijDaneFormularza(int? selectedUserId)
        {
            var users = _context.Uzytkownik.AsNoTracking()
                .OrderBy(u => u.Email)
                .Select(u => new { u.IdUzytkownika, u.Email })
                .ToList();

            var list = users.Select(u => new SelectListItem
            {
                Value = u.IdUzytkownika.ToString(),
                Text = u.Email,
                Selected = selectedUserId == u.IdUzytkownika
            }).ToList();
            list.Insert(0, new SelectListItem { Value = string.Empty, Text = "(System / brak uzytkownika)", Selected = selectedUserId == null });

            ViewData["UserId"] = list;
        }

        private static void NormalizujLog(LogAudytu logAudytu, bool normalizeLocalTimeToUtc)
        {
            logAudytu.Akcja = (logAudytu.Akcja ?? string.Empty).Trim().ToUpperInvariant();
            logAudytu.Encja = (logAudytu.Encja ?? string.Empty).Trim();
            logAudytu.IdEncji = string.IsNullOrWhiteSpace(logAudytu.IdEncji) ? null : logAudytu.IdEncji.Trim();
            logAudytu.StareJson = string.IsNullOrWhiteSpace(logAudytu.StareJson) ? null : logAudytu.StareJson.Trim();
            logAudytu.NoweJson = string.IsNullOrWhiteSpace(logAudytu.NoweJson) ? null : logAudytu.NoweJson.Trim();

            if (normalizeLocalTimeToUtc)
            {
                logAudytu.KiedyUtc = DateTime.SpecifyKind(logAudytu.KiedyUtc, DateTimeKind.Local).ToUniversalTime();
            }
        }
    }
}
