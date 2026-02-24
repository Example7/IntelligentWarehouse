using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IDokumentWZService
    {
        Task<DokumentWZIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<DokumentWZDetailsDto?> GetDetailsDataAsync(int idDokumentu);
        Task<DokumentWZDeleteDto?> GetDeleteDataAsync(int idDokumentu);
        Task<DokumentWZPostResultDto> PostAsync(int idDokumentu);
    }
}
