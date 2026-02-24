using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPozycjaRezerwacjiService
    {
        Task<PozycjaRezerwacjiIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PozycjaRezerwacjiDetailsDto?> GetDetailsDataAsync(int id);
        Task<PozycjaRezerwacjiDeleteDto?> GetDeleteDataAsync(int id);
    }
}
