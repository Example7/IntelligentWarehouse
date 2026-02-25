using Data.Data.CMS;
using Interfaces.CMS.Dtos;

namespace Interfaces.CMS
{
    public interface IStronaService
    {
        Task<IList<Strona>> GetStronyByPosition();

        Task<Strona?> GetStronaById(int? id);

        Task<StronaIndexDto> GetIndexDataAsync(string? searchTerm);

        Task<StronaDetailsDto?> GetDetailsDataAsync(int id);

        Task<StronaDeleteDto?> GetDeleteDataAsync(int id);

        Task<bool> TytulLinkuExistsAsync(string tytulLinku, int? excludeId = null);
    }
}
