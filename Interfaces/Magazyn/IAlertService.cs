using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IAlertService
    {
        Task<AlertIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<AlertDetailsDto?> GetDetailsDataAsync(long idAlertu);
        Task<AlertDeleteDto?> GetDeleteDataAsync(long idAlertu);
        Task<AlertGenerateFromRulesResultDto> GenerujAlertyZRegulAsync();
    }
}
