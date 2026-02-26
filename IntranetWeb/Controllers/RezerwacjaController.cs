using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Controllers.Abstrakcja;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynier)]
    public class RezerwacjaController : BaseSearchController<Rezerwacja>
    {
        private readonly IRezerwacjaService _rezerwacjaService;
        private static readonly string[] DozwoloneStatusyCreate = ["Draft"];
        private static readonly string[] DozwoloneStatusyEdit = ["Draft", "Active", "Completed", "Cancelled"];

        public RezerwacjaController(DataContext context, IRezerwacjaService rezerwacjaService, IAlertService alertService) : base(context)
        {
            _rezerwacjaService = rezerwacjaService;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _rezerwacjaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var model = await _rezerwacjaService.GetDetailsDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _rezerwacjaService.ActivateAsync(id);
            if (result.Success)
            {
                TempData["RezerwacjaStatusSuccess"] = result.Message;
            }
            else
            {
                TempData["RezerwacjaStatusError"] = result.Message;
                if (result.Errors.Count > 0)
                {
                    TempData["RezerwacjaStatusErrorDetails"] = string.Join(" | ", result.Errors.Select(e => e.Error));
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int id)
        {
            var result = await _rezerwacjaService.ReleaseAsync(id);
            if (result.Success)
            {
                TempData["RezerwacjaStatusSuccess"] = result.Message;
            }
            else
            {
                TempData["RezerwacjaStatusError"] = result.Message;
                if (result.Errors.Count > 0)
                {
                    TempData["RezerwacjaStatusErrorDetails"] = string.Join(" | ", result.Errors.Select(e => e.Error));
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWzFromReservation(int id)
        {
            var reservation = await _context.Rezerwacja
                .Include(r => r.Pozycje.OrderBy(p => p.Lp))
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (!string.Equals(reservation.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                TempData["RezerwacjaStatusError"] = "WZ można utworzyć tylko z aktywnej rezerwacji.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (reservation.Pozycje.Count == 0)
            {
                TempData["RezerwacjaStatusError"] = "Rezerwacja nie ma pozycji. Nie można utworzyć WZ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var customerId = await _context.Klient
                .AsNoTracking()
                .Where(k => k.IdUzytkownika == reservation.IdUtworzyl && k.CzyAktywny)
                .Select(k => (int?)k.IdKlienta)
                .FirstOrDefaultAsync();

            if (!customerId.HasValue)
            {
                TempData["RezerwacjaStatusError"] = "Nie znaleziono aktywnego klienta powiązanego z autorem rezerwacji. Nie można utworzyć WZ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentUserId = TryGetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                TempData["RezerwacjaStatusError"] = "Nie można ustalić zalogowanego użytkownika.";
                return RedirectToAction(nameof(Details), new { id });
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;
                var reservationItems = reservation.Pozycje.OrderBy(p => p.Lp).ThenBy(p => p.Id).ToList();
                var productIds = reservationItems.Select(p => p.IdProduktu).Distinct().ToList();
                var stockRows = await _context.StanMagazynowy
                    .AsNoTracking()
                    .Include(s => s.Lokacja)
                    .Where(s =>
                        productIds.Contains(s.IdProduktu) &&
                        s.Lokacja.IdMagazynu == reservation.IdMagazynu &&
                        s.Lokacja.CzyAktywna)
                    .ToListAsync();

                var availableByKey = stockRows
                    .GroupBy(s => (s.IdProduktu, s.IdLokacji))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Ilosc));

                var wz = new DokumentWZ
                {
                    Numer = await GenerateNextWzNumberAsync(utcNow.Year),
                    IdMagazynu = reservation.IdMagazynu,
                    IdKlienta = customerId.Value,
                    Status = "Draft",
                    DataWydaniaUtc = utcNow,
                    IdUtworzyl = currentUserId.Value,
                    ZaksiegowanoUtc = null,
                    Notatka = BuildWzNoteFromReservation(reservation)
                };

                foreach (var item in reservationItems)
                {
                    int? targetLocationId = item.IdLokacji;

                    if (targetLocationId.HasValue)
                    {
                        var explicitKey = (item.IdProduktu, targetLocationId.Value);
                        if (!availableByKey.TryGetValue(explicitKey, out var explicitAvailable))
                        {
                            await tx.RollbackAsync();
                            TempData["RezerwacjaStatusError"] =
                                $"Pozycja {item.Lp} ({item.IdProduktu}) wskazuje lokację spoza magazynu rezerwacji albo bez stanu.";
                            return RedirectToAction(nameof(Details), new { id });
                        }

                        if (explicitAvailable < item.Ilosc)
                        {
                            await tx.RollbackAsync();
                            TempData["RezerwacjaStatusError"] =
                                $"Pozycja {item.Lp} nie może zostać przeniesiona do WZ. Brak wystarczającego stanu we wskazanej lokacji.";
                            return RedirectToAction(nameof(Details), new { id });
                        }

                        availableByKey[explicitKey] = explicitAvailable - item.Ilosc;

                        wz.Pozycje.Add(new PozycjaWZ
                        {
                            Lp = wz.Pozycje.Count + 1,
                            IdProduktu = item.IdProduktu,
                            IdLokacji = targetLocationId,
                            IdPartii = null,
                            Ilosc = item.Ilosc
                        });
                        continue;
                    }

                    var productLocations = availableByKey
                        .Where(x => x.Key.IdProduktu == item.IdProduktu && x.Value > 0)
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => x.Key.IdLokacji)
                        .ToList();

                    var totalAvailableForProduct = productLocations.Sum(x => x.Value);
                    if (totalAvailableForProduct < item.Ilosc)
                    {
                        await tx.RollbackAsync();
                        TempData["RezerwacjaStatusError"] =
                            $"Pozycja {item.Lp} nie może zostać przeniesiona do WZ. Łączny dostępny stan w magazynie jest mniejszy niż ilość rezerwacji.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    var remaining = item.Ilosc;
                    foreach (var locationStock in productLocations)
                    {
                        if (remaining <= 0)
                        {
                            break;
                        }

                        var allocate = Math.Min(remaining, locationStock.Value);
                        if (allocate <= 0)
                        {
                            continue;
                        }

                        wz.Pozycje.Add(new PozycjaWZ
                        {
                            Lp = wz.Pozycje.Count + 1,
                            IdProduktu = item.IdProduktu,
                            IdLokacji = locationStock.Key.IdLokacji,
                            IdPartii = null,
                            Ilosc = allocate
                        });

                        availableByKey[locationStock.Key] = locationStock.Value - allocate;
                        remaining -= allocate;
                    }

                    if (remaining > 0)
                    {
                        await tx.RollbackAsync();
                        TempData["RezerwacjaStatusError"] =
                            $"Pozycji {item.Lp} nie udało się rozdzielić na lokacje podczas tworzenia WZ. Ustaw lokacje ręcznie.";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }

                _context.DokumentWZ.Add(wz);
                reservation.Status = "Released";
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["DokumentWZCreateSuccess"] = $"Utworzono dokument WZ {wz.Numer} na podstawie rezerwacji {reservation.Numer}.";
                return RedirectToAction("Details", "DokumentWZ", new { id = wz.Id });
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["RezerwacjaStatusError"] = $"Nie udało się utworzyć WZ: {ex.GetBaseException().Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public IActionResult Create()
        {
            var model = new Rezerwacja
            {
                Status = "Draft",
                UtworzonoUtc = DateTime.UtcNow
            };
            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numer,IdMagazynu,WygasaUtc,IdUtworzyl,Notatka")] Rezerwacja rezerwacja)
        {
            rezerwacja.Status = "Draft";
            rezerwacja.UtworzonoUtc = DateTime.UtcNow;
            rezerwacja.Numer = rezerwacja.Numer?.Trim() ?? string.Empty;
            rezerwacja.WygasaUtc = NormalizeLocalInputToUtc(rezerwacja.WygasaUtc);

            if (await CzyNumerRezerwacjiJuzIstniejeAsync(rezerwacja.Numer))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(rezerwacja);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
                }
            }

            UzupelnijDaneFormularza(rezerwacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyCreate, rezerwacja.Status);
            return View(rezerwacja);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.Utworzyl)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rezerwacja == null) return NotFound();

            UzupelnijDaneFormularza(rezerwacja);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, rezerwacja.Status);
            ViewData["AutorDokumentuEmail"] = rezerwacja.Utworzyl?.Email ?? "-";
            return View(rezerwacja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numer,IdMagazynu,Status,WygasaUtc,IdUtworzyl,Notatka,RowVersion")] Rezerwacja rezerwacja)
        {
            if (id != rezerwacja.Id) return NotFound();

            rezerwacja.Numer = rezerwacja.Numer?.Trim() ?? string.Empty;
            rezerwacja.WygasaUtc = NormalizeLocalInputToUtc(rezerwacja.WygasaUtc);
            if (!DozwoloneStatusyEdit.Contains(rezerwacja.Status))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Status), "Niedozwolony status rezerwacji.");
            }

            if (await CzyNumerRezerwacjiJuzIstniejeAsync(rezerwacja.Numer, rezerwacja.Id))
            {
                ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Rezerwacja.FirstOrDefaultAsync(r => r.Id == id);
                    if (existing == null) return NotFound();

                    existing.Numer = rezerwacja.Numer;
                    existing.IdMagazynu = rezerwacja.IdMagazynu;
                    existing.Status = rezerwacja.Status;
                    existing.WygasaUtc = rezerwacja.WygasaUtc;
                    existing.Notatka = rezerwacja.Notatka;
                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = rezerwacja.RowVersion;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezerwacjaExists(rezerwacja.Id)) return NotFound();
                    throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Rezerwacja.Numer), $"Rezerwacja o numerze '{rezerwacja.Numer}' już istnieje.");
                }
            }

            return await ReturnEditViewWithLookupsAsync(rezerwacja);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _rezerwacjaService.GetDeleteDataAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _rezerwacjaService.GetDeleteDataAsync(id);
            if (deleteData == null) return NotFound();

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie można usunąć rezerwacji, ponieważ ma powiązane pozycje ({deleteData.LiczbaPozycji}).");
                return View("Delete", deleteData);
            }

            var rezerwacja = await _context.Rezerwacja.FindAsync(id);
            if (rezerwacja != null) _context.Rezerwacja.Remove(rezerwacja);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć rezerwacji, ponieważ ma powiązane rekordy (np. pozycje rezerwacji).");
                var refreshed = await _rezerwacjaService.GetDeleteDataAsync(id);
                return refreshed == null ? NotFound() : View("Delete", refreshed);
            }
        }

        private bool RezerwacjaExists(int id) => _context.Rezerwacja.Any(e => e.Id == id);

        private Task<bool> CzyNumerRezerwacjiJuzIstniejeAsync(string numer, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(numer)) return Task.FromResult(false);
            return _context.Rezerwacja.AsNoTracking().AnyAsync(r => r.Numer == numer && (!excludeId.HasValue || r.Id != excludeId.Value));
        }

        private void UzupelnijDaneFormularza(Rezerwacja model)
        {
            ViewData["IdMagazynu"] = new SelectList(_context.Magazyn.AsNoTracking().OrderBy(x => x.Nazwa), "IdMagazynu", "Nazwa", model.IdMagazynu);
            ViewData["IdUtworzyl"] = new SelectList(_context.Uzytkownik.AsNoTracking().OrderBy(x => x.Email), "IdUzytkownika", "Email", model.IdUtworzyl);
        }

        private async Task<IActionResult> ReturnEditViewWithLookupsAsync(Rezerwacja model)
        {
            UzupelnijDaneFormularza(model);
            ViewData["StatusOptions"] = BuildStatusSelectList(DozwoloneStatusyEdit, model.Status);
            ViewData["AutorDokumentuEmail"] = await _context.Uzytkownik
                .Where(u => u.IdUzytkownika == model.IdUtworzyl)
                .Select(u => u.Email)
                .FirstOrDefaultAsync() ?? "-";
            return View(model);
        }

        private static IReadOnlyList<SelectListItem> BuildStatusSelectList(IEnumerable<string> allowedStatuses, string? selected)
            => allowedStatuses.Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase))).ToList();

        private async Task<string> GenerateNextWzNumberAsync(int year)
        {
            var prefix = $"WZ/{year}/";
            var lastNumber = await _context.DokumentWZ
                .AsNoTracking()
                .Where(d => d.Numer.StartsWith(prefix))
                .OrderByDescending(d => d.Id)
                .Select(d => d.Numer)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var suffix = lastNumber[prefix.Length..];
                if (int.TryParse(suffix, out var parsed))
                {
                    next = parsed + 1;
                }
            }

            return $"{prefix}{next:0000}";
        }

        private int? TryGetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private static string? BuildWzNoteFromReservation(Rezerwacja reservation)
        {
            var source = $"Utworzono z rezerwacji {reservation.Numer}.";
            if (string.IsNullOrWhiteSpace(reservation.Notatka))
            {
                return source;
            }

            var note = reservation.Notatka.Trim();
            return $"{source} Notatka rezerwacji: {note}".Length <= 500
                ? $"{source} Notatka rezerwacji: {note}"
                : source;
        }

        private static DateTime? NormalizeLocalInputToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var dt = value.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            return dt.ToUniversalTime();
        }
    }
}