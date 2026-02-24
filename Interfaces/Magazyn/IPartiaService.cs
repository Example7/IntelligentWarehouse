using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IPartiaService
    {
        Task<PartiaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<PartiaDetailsDto?> GetDetailsDataAsync(int idPartii);
        Task<PartiaDeleteDto?> GetDeleteDataAsync(int idPartii);
    }
}
