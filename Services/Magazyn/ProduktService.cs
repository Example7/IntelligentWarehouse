using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class ProduktService : BaseService, IProduktService
    {
        public ProduktService(DataContext context) : base(context)
        {
        }

        public async Task<Produkt?> GetProdukt(int? idProduktu)
        {
            var produkt = await _context.Produkt.Where(p => p.IdProduktu == idProduktu).FirstOrDefaultAsync();
            return produkt;
        }

        public async Task<IList<Produkt>> GetProdukty(int idKategorii)
        {
            var produkty = await _context.Produkt.Where(p => p.IdProduktu == idKategorii).ToListAsync();
            return produkty;
        }
    }
}
