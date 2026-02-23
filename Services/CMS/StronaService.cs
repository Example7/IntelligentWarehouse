using Data.Data;
using Data.Data.CMS;
using Interfaces.CMS;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class StronaService : BaseService, IStronaService
    {
        public StronaService(DataContext context) : base(context)
        {
        }

        public async Task<Strona?> GetStronaById(int? id)
        {
            var strona = await _context.Strona.FindAsync(id);
            return strona;
        }

        public async Task<IList<Strona>> GetStronyByPosition()
        {
            var strony = await _context.Strona.OrderBy(s => s.Pozycja).ToListAsync();
            return strony;
        }
    }
}
