using Data.Data.CMS;

namespace Interfaces.CMS
{
    public interface IStronaService
    {
        Task<IList<Strona>> GetStronyByPosition();

        Task<Strona?> GetStronaById(int? id);
    }
}
