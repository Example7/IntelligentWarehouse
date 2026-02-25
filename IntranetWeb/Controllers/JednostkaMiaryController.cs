using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using IntranetWeb.Controllers.Abstrakcja;

using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminMagazynierOperator)]
    public class JednostkaMiaryController : BaseSearchController<JednostkaMiary>
    {
        private readonly IJednostkaMiaryService _jednostkaMiaryService;

        public JednostkaMiaryController(DataContext context, IJednostkaMiaryService jednostkaMiaryService) : base(context)
        {
            _jednostkaMiaryService = jednostkaMiaryService;
        }

        // GET: JednostkaMiary
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await _jednostkaMiaryService.GetIndexDataAsync(searchTerm);
            return View(model);
        }

        // GET: JednostkaMiary/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailsData = await _jednostkaMiaryService.GetDetailsDataAsync(id.Value);
            if (detailsData == null)
            {
                return NotFound();
            }

            return View(detailsData);
        }

        // GET: JednostkaMiary/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JednostkaMiary/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdJednostki,Kod,Nazwa,CzyAktywna")] JednostkaMiary jednostkaMiary)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jednostkaMiary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(jednostkaMiary);
        }

        // GET: JednostkaMiary/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jednostkaMiary = await _context.JednostkaMiary.FindAsync(id);
            if (jednostkaMiary == null)
            {
                return NotFound();
            }
            return View(jednostkaMiary);
        }

        // POST: JednostkaMiary/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdJednostki,Kod,Nazwa,CzyAktywna")] JednostkaMiary jednostkaMiary)
        {
            if (id != jednostkaMiary.IdJednostki)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jednostkaMiary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JednostkaMiaryExists(jednostkaMiary.IdJednostki))
                    {
                        return NotFound();
                    }

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(jednostkaMiary);
        }

        // GET: JednostkaMiary/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deleteData = await _jednostkaMiaryService.GetDeleteDataAsync(id.Value);
            if (deleteData == null)
            {
                return NotFound();
            }

            return View(deleteData);
        }

        // POST: JednostkaMiary/Delete/5
        [Authorize(Roles = AppRoles.AdminOnly)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jednostkaMiary = await _context.JednostkaMiary.FindAsync(id);
            if (jednostkaMiary == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var liczbaProduktow = await _jednostkaMiaryService.GetAssignedProductsCountAsync(id);
            if (liczbaProduktow > 0)
            {
                var deleteData = await _jednostkaMiaryService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "Nie mozna usunac jednostki miary, poniewaz jest przypisana do produktow.");
                return View("Delete", deleteData);
            }

            try
            {
                _context.JednostkaMiary.Remove(jednostkaMiary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var deleteData = await _jednostkaMiaryService.GetDeleteDataAsync(id);
                if (deleteData == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Nie udalo sie usunac jednostki miary z powodu istniejacych powiazan.");
                return View("Delete", deleteData);
            }
        }

        private bool JednostkaMiaryExists(int id)
        {
            return _context.JednostkaMiary.Any(e => e.IdJednostki == id);
        }
    }
}


