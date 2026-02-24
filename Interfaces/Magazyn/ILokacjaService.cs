using Data.Data.Magazyn;
using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface ILokacjaService
    {
        Task<IList<Lokacja>> GetLokacje();
        Task<LokacjaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<LokacjaDetailsDto?> GetDetailsDataAsync(int idLokacji);
    }
}
