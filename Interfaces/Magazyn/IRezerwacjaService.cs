using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IRezerwacjaService
    {
        Task<RezerwacjaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<RezerwacjaDetailsDto?> GetDetailsDataAsync(int id);
        Task<RezerwacjaDeleteDto?> GetDeleteDataAsync(int id);
        Task<RezerwacjaStatusChangeResultDto> ActivateAsync(int id);
        Task<RezerwacjaStatusChangeResultDto> ReleaseAsync(int id);
        Task<int> ReleaseExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    }
}
