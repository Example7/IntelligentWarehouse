using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IDokumentPZService
    {
        Task<DokumentPZIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<DokumentPZDetailsDto?> GetDetailsDataAsync(int idDokumentu);
        Task<DokumentPZDeleteDto?> GetDeleteDataAsync(int idDokumentu);
        Task<DokumentPZPostResultDto> PostAsync(int idDokumentu);
    }
}
