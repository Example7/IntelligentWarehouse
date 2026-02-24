using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IDostawcaService
    {
        Task<DostawcaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<DostawcaDetailsDto?> GetDetailsDataAsync(int idDostawcy);
        Task<DostawcaDeleteDto?> GetDeleteDataAsync(int idDostawcy);
    }
}
