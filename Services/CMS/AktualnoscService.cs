using Data.Data;
using Data.Data.CMS;
using Interfaces.CMS;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.CMS
{
    public class AktualnoscService : BaseService, IAktualnoscService
    {
        public AktualnoscService(DataContext context) : base(context)
        {
        }

        public async Task<Aktualnosc?> GetAktualnoscById(int? id)
        {
            var aktualnosc = await _context.Aktualnosc.FirstOrDefaultAsync(a => a.IdAktualnosci == id);
            return aktualnosc;
        }

        public async Task<IList<Aktualnosc>> GetAktualnosciByPosition(int number)
        {
            var aktualnosci = await _context.Aktualnosc.OrderByDescending(a => a.Pozycja).Take(number).ToListAsync();
            return aktualnosci;
        }
    }
}
