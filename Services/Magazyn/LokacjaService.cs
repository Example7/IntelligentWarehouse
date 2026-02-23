using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class LokacjaService : BaseService, ILokacjaService
    {
        public LokacjaService(DataContext context) : base(context) { }

        public async Task<IList<Lokacja>> GetLokacje()
        {
            return await _context.Lokacja
                .Include(l => l.Magazyn)
                .Include(l => l.Stany)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}