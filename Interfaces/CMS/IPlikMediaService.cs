using Interfaces.CMS.Dtos;

namespace Interfaces.CMS
{
    public interface IPlikMediaService
    {
        Task<PlikMediaIndexDto> GetIndexDataAsync(string? searchTerm);

        Task<PlikMediaDetailsDto?> GetDetailsDataAsync(long id);

        Task<PlikMediaDeleteDto?> GetDeleteDataAsync(long id);
    }
}
