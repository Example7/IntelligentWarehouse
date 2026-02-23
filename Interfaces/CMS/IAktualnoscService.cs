using Data.Data.CMS;

namespace Interfaces.CMS
{
    public interface IAktualnoscService
    {
        Task<IList<Aktualnosc>> GetAktualnosciByPosition(int number);

        Task<Aktualnosc?> GetAktualnoscById(int? id);
    }
}
