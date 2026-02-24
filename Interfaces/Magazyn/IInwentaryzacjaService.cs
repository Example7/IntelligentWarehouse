using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IInwentaryzacjaService
    {
        Task<InwentaryzacjaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<InwentaryzacjaDetailsDto?> GetDetailsDataAsync(int idDokumentu);
        Task<InwentaryzacjaDeleteDto?> GetDeleteDataAsync(int idDokumentu);
        Task<InwentaryzacjaCloseResultDto> CloseAsync(int idDokumentu);
    }
}
