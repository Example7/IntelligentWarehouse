using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IProduktJednostkaService
    {
        Task<ProduktJednostkaIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<ProduktJednostkaDetailsDto?> GetDetailsDataAsync(int id);
        Task<ProduktJednostkaDeleteDto?> GetDeleteDataAsync(int id);
    }
}
