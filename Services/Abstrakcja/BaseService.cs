using Data.Data;

namespace Services.Abstrakcja
{
    public abstract class BaseService
    {
        protected readonly DataContext _context;

        public BaseService(DataContext context)
        {
            _context = context;
        }
    }
}
