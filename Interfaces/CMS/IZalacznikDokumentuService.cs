using Interfaces.CMS.Dtos;

namespace Interfaces.CMS
{
    public interface IZalacznikDokumentuService
    {
        Task<ZalacznikDokumentuIndexDto> GetIndexDataAsync(string? searchTerm);

        Task<ZalacznikDokumentuDetailsDto?> GetDetailsDataAsync(long id);

        Task<ZalacznikDokumentuDeleteDto?> GetDeleteDataAsync(long id);
    }
}
