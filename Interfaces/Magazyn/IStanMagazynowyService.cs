using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IStanMagazynowyService
    {
        Task<StanMagazynowyIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<StanMagazynowyFormDto> GetCreateFormAsync();
        Task<StanMagazynowyFormDto?> GetEditFormAsync(int idStanu);
        Task<StanMagazynowyFormDto> PrepareFormAsync(Data.Data.Magazyn.StanMagazynowy stanMagazynowy, bool isEdit);
    }
}
