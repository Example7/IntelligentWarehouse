using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class KategoriaService : BaseService, IKategoriaService
    {
        public KategoriaService(DataContext context) : base(context)
        {
        }

        public async Task<IList<Kategoria>> GetKategorie()
        {
            return await _context.Kategoria.ToListAsync();
        }

        public async Task<KategoriaDetailsDto?> GetDetailsDataAsync(int idKategorii)
        {
            var kategoria = await _context.Kategoria
                .AsNoTracking()
                .Include(k => k.NadrzednaKategoria)
                .FirstOrDefaultAsync(k => k.IdKategorii == idKategorii);

            if (kategoria == null)
            {
                return null;
            }

            var produktyKategorii = await _context.Produkt
                .AsNoTracking()
                .Where(p => p.IdKategorii == idKategorii)
                .OrderBy(p => p.Kod)
                .ThenBy(p => p.Nazwa)
                .ToListAsync();

            var liczbaPodkategorii = await _context.Kategoria.CountAsync(k => k.IdNadrzednejKategorii == idKategorii);

            return new KategoriaDetailsDto
            {
                Kategoria = kategoria,
                ProduktyKategorii = produktyKategorii,
                LiczbaProduktow = produktyKategorii.Count,
                LiczbaPodkategorii = liczbaPodkategorii
            };
        }

        public async Task<KategoriaDeleteDto?> GetDeleteDataAsync(int idKategorii)
        {
            var kategoria = await _context.Kategoria
                .AsNoTracking()
                .Include(k => k.NadrzednaKategoria)
                .FirstOrDefaultAsync(k => k.IdKategorii == idKategorii);

            if (kategoria == null)
            {
                return null;
            }

            var (liczbaProduktow, liczbaPodkategorii) = await GetDeleteBlockersAsync(idKategorii);

            return new KategoriaDeleteDto
            {
                Kategoria = kategoria,
                LiczbaProduktow = liczbaProduktow,
                LiczbaPodkategorii = liczbaPodkategorii,
                CzyMoznaUsunac = liczbaProduktow == 0 && liczbaPodkategorii == 0
            };
        }

        public async Task<(int liczbaProduktow, int liczbaPodkategorii)> GetDeleteBlockersAsync(int idKategorii)
        {
            var liczbaProduktow = await _context.Produkt.CountAsync(p => p.IdKategorii == idKategorii);
            var liczbaPodkategorii = await _context.Kategoria.CountAsync(k => k.IdNadrzednejKategorii == idKategorii);
            return (liczbaProduktow, liczbaPodkategorii);
        }
    }
}
