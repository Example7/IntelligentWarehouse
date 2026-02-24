using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IDokumentMMService
    {
        Task<DokumentMMIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<DokumentMMDetailsDto?> GetDetailsDataAsync(int idDokumentu);
        Task<DokumentMMDeleteDto?> GetDeleteDataAsync(int idDokumentu);
        Task<DokumentMMPostResultDto> PostAsync(int idDokumentu);
    }
}
