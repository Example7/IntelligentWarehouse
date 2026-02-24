using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IMagazynService
    {
        Task<MagazynIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<MagazynDetailsDto?> GetMagazynDetailsData(int idMagazynu);
    }
}
