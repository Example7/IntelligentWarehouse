using Data.Data;
using Interfaces.CMS;
using Interfaces.CMS.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class ZalacznikDokumentuService : BaseService, IZalacznikDokumentuService
    {
        public ZalacznikDokumentuService(DataContext context) : base(context)
        {
        }

        public async Task<ZalacznikDokumentuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var term = (searchTerm ?? string.Empty).Trim();
            var query = _context.ZalacznikDokumentu
                .AsNoTracking()
                .Include(x => x.Wgral)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.TypDokumentu.Contains(term) ||
                    x.NazwaPliku.Contains(term) ||
                    x.ContentType.Contains(term) ||
                    x.Sciezka.Contains(term));
            }

            var items = await query
                .OrderByDescending(x => x.WgranoUtc)
                .Select(x => new ZalacznikDokumentuIndexItemDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    IdDokumentu = x.IdDokumentu,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .ToListAsync();

            return new ZalacznikDokumentuIndexDto
            {
                SearchTerm = term,
                Items = items
            };
        }

        public async Task<ZalacznikDokumentuDetailsDto?> GetDetailsDataAsync(long id)
        {
            return await _context.ZalacznikDokumentu
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new ZalacznikDokumentuDetailsDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    IdDokumentu = x.IdDokumentu,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    WgranoUtc = x.WgranoUtc,
                    WgralUserId = x.WgralUserId,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ZalacznikDokumentuDeleteDto?> GetDeleteDataAsync(long id)
        {
            return await _context.ZalacznikDokumentu
                .AsNoTracking()
                .Include(x => x.Wgral)
                .Where(x => x.Id == id)
                .Select(x => new ZalacznikDokumentuDeleteDto
                {
                    Id = x.Id,
                    TypDokumentu = x.TypDokumentu,
                    IdDokumentu = x.IdDokumentu,
                    NazwaPliku = x.NazwaPliku,
                    ContentType = x.ContentType,
                    Sciezka = x.Sciezka,
                    WgranoUtc = x.WgranoUtc,
                    WgralEmail = x.Wgral != null ? x.Wgral.Email : null
                })
                .FirstOrDefaultAsync();
        }
    }
}
