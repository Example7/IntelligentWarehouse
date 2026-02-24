using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPozycjaWZService
    {
        Task<PozycjaWZIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PozycjaWZDetailsDto?> GetDetailsDataAsync(int idPozycji);
        Task<PozycjaWZDeleteDto?> GetDeleteDataAsync(int idPozycji);
    }
}
