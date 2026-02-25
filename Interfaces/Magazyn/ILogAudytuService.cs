using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface ILogAudytuService
    {
        Task<LogAudytuIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<LogAudytuDetailsDto?> GetDetailsDataAsync(long idLogu);
        Task<LogAudytuDeleteDto?> GetDeleteDataAsync(long idLogu);
    }
}
