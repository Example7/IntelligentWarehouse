using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPozycjaMMService
    {
        Task<PozycjaMMIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PozycjaMMDetailsDto?> GetDetailsDataAsync(int idPozycji);
        Task<PozycjaMMDeleteDto?> GetDeleteDataAsync(int idPozycji);
    }
}
