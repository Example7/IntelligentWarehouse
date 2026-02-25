using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Data.Data.CMS;
using IntranetWeb.Controllers.Abstrakcja;
using Interfaces.CMS;
using System.Text.Json;

namespace IntranetWeb.Controllers
{
    public class ZalacznikDokumentuController : BaseSearchController<ZalacznikDokumentu>
    {
        private readonly IZalacznikDokumentuService _zalacznikDokumentuService;

        public ZalacznikDokumentuController(DataContext context, IZalacznikDokumentuService zalacznikDokumentuService) : base(context)
        {
            _zalacznikDokumentuService = zalacznikDokumentuService;
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
            PopulateUzytkownicySelect();
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
        public async Task<IActionResult> Create([Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka,WgralUserId")] ZalacznikDokumentu zalacznikDokumentu)
        {
            Normalize(zalacznikDokumentu);
            zalacznikDokumentu.WgranoUtc = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(zalacznikDokumentu.WgralUserId);
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
            PopulateUzytkownicySelect(zalacznikDokumentu.WgralUserId);
            PopulatePowiazaneDokumentySelectData();
            return View(zalacznikDokumentu);
        }

        // POST: ZalacznikDokumentu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TypDokumentu,IdDokumentu,NazwaPliku,ContentType,Sciezka,WgralUserId")] ZalacznikDokumentu zalacznikDokumentu)
        {
            if (id != zalacznikDokumentu.Id)
            {
                return NotFound();
            }

            Normalize(zalacznikDokumentu);

            if (!ModelState.IsValid)
            {
                PopulateUzytkownicySelect(zalacznikDokumentu.WgralUserId);
                PopulatePowiazaneDokumentySelectData();
                return View(zalacznikDokumentu);
            }

            var existing = await _context.ZalacznikDokumentu.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TypDokumentu = zalacznikDokumentu.TypDokumentu;
            existing.IdDokumentu = zalacznikDokumentu.IdDokumentu;
            existing.NazwaPliku = zalacznikDokumentu.NazwaPliku;
            existing.ContentType = zalacznikDokumentu.ContentType;
            existing.Sciezka = zalacznikDokumentu.Sciezka;
            existing.WgralUserId = zalacznikDokumentu.WgralUserId;

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
            zalacznikDokumentu.TypDokumentu = (zalacznikDokumentu.TypDokumentu ?? string.Empty).Trim();
            zalacznikDokumentu.NazwaPliku = (zalacznikDokumentu.NazwaPliku ?? string.Empty).Trim();
            zalacznikDokumentu.ContentType = (zalacznikDokumentu.ContentType ?? string.Empty).Trim();
            zalacznikDokumentu.Sciezka = (zalacznikDokumentu.Sciezka ?? string.Empty).Trim();
        }
    }
}
