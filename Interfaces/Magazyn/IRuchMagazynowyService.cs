using Interfaces.Magazyn.Dtos;
using Data.Data.Magazyn;

namespace Interfaces.Magazyn
{
    public interface IRuchMagazynowyService
    {
        Task<RuchMagazynowyIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<RuchMagazynowyCreateResultDto> CreateAndApplyAsync(RuchMagazynowy ruchMagazynowy);
        Task<RuchMagazynowyCreateResultDto> UpdateAndReapplyAsync(int idRuchu, RuchMagazynowy ruchMagazynowy);
        Task<RuchMagazynowyCreateResultDto> DeleteAndRevertAsync(int idRuchu);
    }
}
