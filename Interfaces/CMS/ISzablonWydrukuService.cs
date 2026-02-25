using Interfaces.CMS.Dtos;

namespace Interfaces.CMS
{
    public interface ISzablonWydrukuService
    {
        Task<SzablonWydrukuIndexDto> GetIndexDataAsync(string? searchTerm);

        Task<SzablonWydrukuDetailsDto?> GetDetailsDataAsync(int id);

        Task<SzablonWydrukuDeleteDto?> GetDeleteDataAsync(int id);

        Task<bool> TypIWersjaExistsAsync(string typDokumentu, string wersja, int? excludeId = null);
    }
}
