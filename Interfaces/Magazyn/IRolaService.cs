using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IRolaService
    {
        Task<RolaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<RolaDetailsDto?> GetDetailsDataAsync(int idRoli);
        Task<RolaDeleteDto?> GetDeleteDataAsync(int idRoli);
    }
}
