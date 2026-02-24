using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPozycjaInwentaryzacjiService
    {
        Task<PozycjaInwentaryzacjiIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PozycjaInwentaryzacjiDetailsDto?> GetDetailsDataAsync(int idPozycji);
        Task<PozycjaInwentaryzacjiDeleteDto?> GetDeleteDataAsync(int idPozycji);
    }
}
