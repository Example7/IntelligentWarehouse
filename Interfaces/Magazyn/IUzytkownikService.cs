using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IUzytkownikService
    {
        Task<UzytkownikIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<UzytkownikDetailsDto?> GetDetailsDataAsync(int idUzytkownika);
        Task<UzytkownikDeleteDto?> GetDeleteDataAsync(int idUzytkownika);
    }
}
