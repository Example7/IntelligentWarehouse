using Microsoft.AspNetCore.Mvc;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using IntranetWeb.Security;
using Microsoft.AspNetCore.Authorization;

using IntranetWeb.Controllers.Abstrakcja;

namespace IntranetWeb.Controllers
{
    [Authorize(Roles = AppRoles.AdminOnly)]
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
            return NotFound();
        }

        // POST: LogAudytu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            return NotFound();
        }

        // GET: LogAudytu/Edit/5
        public IActionResult Edit(long? id)
        {
            return NotFound();
        }

        // POST: LogAudytu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, [Bind("Id,UserId,Akcja,Encja,IdEncji,KiedyUtc,StareJson,NoweJson")] LogAudytu logAudytu)
        {
            return NotFound();
        }

        // GET: LogAudytu/Delete/5
        public IActionResult Delete(long? id)
        {
            return NotFound();
        }

        // POST: LogAudytu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            return NotFound();
        }

    }
}
