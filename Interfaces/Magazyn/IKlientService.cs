using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IKlientService
    {
        Task<KlientIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<KlientDetailsDto?> GetDetailsDataAsync(int idKlienta);
        Task<KlientDeleteDto?> GetDeleteDataAsync(int idKlienta);
    }
}
