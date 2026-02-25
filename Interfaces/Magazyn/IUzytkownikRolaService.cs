using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IUzytkownikRolaService
    {
        Task<UzytkownikRolaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<UzytkownikRolaDetailsDto?> GetDetailsDataAsync(int id);
        Task<UzytkownikRolaDeleteDto?> GetDeleteDataAsync(int id);
    }
}
