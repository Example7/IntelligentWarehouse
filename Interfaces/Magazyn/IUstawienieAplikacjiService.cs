using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IUstawienieAplikacjiService
    {
        Task<UstawienieAplikacjiIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<UstawienieAplikacjiDetailsDto?> GetDetailsDataAsync(string klucz);
        Task<UstawienieAplikacjiDeleteDto?> GetDeleteDataAsync(string klucz);
    }
}
