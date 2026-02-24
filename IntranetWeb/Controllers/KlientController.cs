using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    public class KlientController : BaseSearchController<Klient>
    {
        private readonly IKlientService _klientService;

        public KlientController(DataContext context, IKlientService klientService) : base(context)
        {
            _klientService = klientService;
        }

        // GET: Klient
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _klientService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: Klient/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _klientService.GetDetailsDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Klient/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Klient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKlienta,Nazwa,Email,Telefon,Adres,CzyAktywny")] Klient klient)
        {
            if (ModelState.IsValid)
            {
                klient.UtworzonoUtc = DateTime.UtcNow;
                _context.Add(klient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(klient);
        }

        // GET: Klient/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var klient = await _context.Klient.FindAsync(id);
            if (klient == null)
            {
                return NotFound();
            }
            return View(klient);
        }

        // POST: Klient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKlienta,Nazwa,Email,Telefon,Adres,CzyAktywny,RowVersion")] Klient klient)
        {
            if (id != klient.IdKlienta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Klient.FirstOrDefaultAsync(x => x.IdKlienta == id);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.Nazwa = klient.Nazwa;
                    existing.Email = klient.Email;
                    existing.Telefon = klient.Telefon;
                    existing.Adres = klient.Adres;
                    existing.CzyAktywny = klient.CzyAktywny;

                    _context.Entry(existing).Property(x => x.RowVersion).OriginalValue = klient.RowVersion;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KlientExists(klient.IdKlienta))
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
            return View(klient);
        }

        // GET: Klient/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _klientService.GetDeleteDataAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Klient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteData = await _klientService.GetDeleteDataAsync(id);
            if (deleteData == null)
            {
                return NotFound();
            }

            if (!deleteData.CzyMoznaUsunac)
            {
                ModelState.AddModelError(string.Empty, $"Nie mozna usunac klienta, poniewaz ma powiazane dokumenty WZ: {deleteData.LiczbaDokumentowWz}.");
                return View("Delete", deleteData);
            }

            var klient = await _context.Klient.FindAsync(id);
            if (klient != null)
            {
                _context.Klient.Remove(klient);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Nie udalo sie usunac klienta, poniewaz ma powiazane dokumenty WZ.");
                deleteData = await _klientService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return NotFound();
                }
                return View("Delete", deleteData);
            }
        }

        private bool KlientExists(int id)
        {
            return _context.Klient.Any(e => e.IdKlienta == id);
        }
    }
}
