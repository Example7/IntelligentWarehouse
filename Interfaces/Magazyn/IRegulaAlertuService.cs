using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IRegulaAlertuService
    {
        Task<RegulaAlertuIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<RegulaAlertuDetailsDto?> GetDetailsDataAsync(int idReguly);
        Task<RegulaAlertuDeleteDto?> GetDeleteDataAsync(int idReguly);
    }
}
