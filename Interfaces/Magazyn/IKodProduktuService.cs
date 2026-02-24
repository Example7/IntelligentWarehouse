using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IKodProduktuService
    {
        Task<KodProduktuIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<KodProduktuDetailsDto?> GetDetailsDataAsync(int idKodu);
        Task<KodProduktuDeleteDto?> GetDeleteDataAsync(int idKodu);
    }
}
