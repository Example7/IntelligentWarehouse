using Data.Data.CMS;
using Interfaces.CMS.Dtos;

namespace Interfaces.CMS
{
    public interface IAktualnoscService
    {
        Task<IList<Aktualnosc>> GetAktualnosciByPosition(int number);

        Task<Aktualnosc?> GetAktualnoscById(int? id);

        Task<AktualnoscIndexDto> GetIndexDataAsync(string? searchTerm);

        Task<AktualnoscDetailsDto?> GetDetailsDataAsync(int id);

        Task<AktualnoscDeleteDto?> GetDeleteDataAsync(int id);

        Task<bool> TytulLinkuExistsAsync(string tytulLinku, int? excludeId = null);
    }
}
