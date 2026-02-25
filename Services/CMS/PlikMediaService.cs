using Data.Data;
using Interfaces.CMS;
using Interfaces.CMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class PlikMediaService : BaseService, IPlikMediaService
    {
        public PlikMediaService(DataContext context) : base(context)
        {
        }

        public async Task<PlikMediaIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var term = (searchTerm ?? string.Empty).Trim();
            var query = _context.PlikMedia
                .AsNoTracking()
                .Include(x => x.Wgral)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.NazwaPliku.Contains(term) ||
                    x.ContentType.Contains(term) ||
                    x.Sciezka.Contains(term) ||
                    (x.Opis != null && x.Opis.Contains(term)));
            }

            var items = await query
                .OrderByDescending(x => x.WgranoUtc)
                .Select(x => new PlikMediaIndexItemDto
                {
                    Id = x.Id,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    RozmiarBajty = x.RozmiarBajty,
                    Opis = x.Opis,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .ToListAsync();

            return new PlikMediaIndexDto
            {
                SearchTerm = term,
                SumaRozmiaruBajty = items.Sum(x => x.RozmiarBajty),
                Items = items
            };
        }

        public async Task<PlikMediaDetailsDto?> GetDetailsDataAsync(long id)
        {
            return await _context.PlikMedia
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new PlikMediaDetailsDto
                {
                    Id = x.Id,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    RozmiarBajty = x.RozmiarBajty,
                    Opis = x.Opis,
                    WgranoUtc = x.WgranoUtc,
                    WgralUserId = x.WgralUserId,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PlikMediaDeleteDto?> GetDeleteDataAsync(long id)
        {
            return await _context.PlikMedia
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new PlikMediaDeleteDto
                {
                    Id = x.Id,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    RozmiarBajty = x.RozmiarBajty,
                    Opis = x.Opis,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }
    }
}
