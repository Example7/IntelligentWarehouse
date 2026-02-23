using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
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
            var kategorie = await _context.Kategoria.ToListAsync();
            return kategorie;
        }
    }
}
