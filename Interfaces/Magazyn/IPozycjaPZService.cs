using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPozycjaPZService
    {
        Task<PozycjaPZIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PozycjaPZDetailsDto?> GetDetailsDataAsync(int idPozycji);
        Task<PozycjaPZDeleteDto?> GetDeleteDataAsync(int idPozycji);
    }
}
