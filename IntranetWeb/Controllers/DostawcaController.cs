
using Microsoft.AspNetCore.Mvc;
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
    public class DostawcaController : BaseSearchController<Dostawca>
    {
        private readonly IDostawcaService _dostawcaService;

        public DostawcaController(DataContext context, IDostawcaService dostawcaService) : base(context)
        {
            _dostawcaService = dostawcaService;
        }

        // GET: Dostawca
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _dostawcaService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Dostawca/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dostawcaService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Dostawca/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dostawca/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDostawcy,Nazwa,NIP,Email,Telefon,Adres,CzyAktywny")] Dostawca dostawca)
        {
            if (ModelState.IsValid)
            {
                dostawca.UtworzonoUtc = DateTime.UtcNow;
                _context.Add(dostawca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dostawca);
        }

        // GET: Dostawca/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dostawca = await _context.Dostawca.FindAsync(id);
            if (dostawca == null)
            {
                return NotFound();
            }
            return View(dostawca);
        }

        // POST: Dostawca/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDostawcy,Nazwa,NIP,Email,Telefon,Adres,CzyAktywny,RowVersion")] Dostawca dostawca)
        {
            if (id != dostawca.IdDostawcy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Dostawca.FirstOrDefaultAsync(x => x.IdDostawcy == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Nazwa = dostawca.Nazwa;
                    existing.NIP = dostawca.NIP;
                    existing.Email = dostawca.Email;
                    existing.Telefon = dostawca.Telefon;
                    existing.Adres = dostawca.Adres;
                    existing.CzyAktywny = dostawca.CzyAktywny;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = dostawca.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DostawcaExists(dostawca.IdDostawcy))
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
            return View(dostawca);
        }

        // GET: Dostawca/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dostawcaService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Dostawca/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _dostawcaService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                var blockers = new List<string>();
                if (deleteData.LiczbaDokumentowPz > 0) blockers.Add($"dokumenty PZ: {deleteData.LiczbaDokumentowPz}");
                if (deleteData.LiczbaPartii > 0) blockers.Add($"partie: {deleteData.LiczbaPartii}");
                ModelState.AddModelError(string.Empty, $"Nie mozna usunac dostawcy, poniewaz ma powiazane rekordy ({string.Join(", ", blockers)}).");
                return View("Delete", deleteData);
            }

            var dostawca = await _context.Dostawca.FindAsync(id);
            if (dostawca != null)
            {
                _context.Dostawca.Remove(dostawca);
            }
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udalo sie usunac dostawcy, poniewaz ma powiazane rekordy (np. dokumenty PZ lub partie).");
                deleteData = await _dostawcaService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return NotFound();
                }
                return View("Delete", deleteData);
            }
        }

        private bool DostawcaExists(int id)
        {
            return _context.Dostawca.Any(e => e.IdDostawcy == id);
        }
    }
}



