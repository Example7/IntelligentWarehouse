using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IRaportMagazynowyService
    {
        Task<RaportStanyMagazynoweDto> GetRaportStanowAsync(string? searchTerm, int? idMagazynu);
        Task<byte[]> ExportRaportStanowExcelAsync(string? searchTerm, int? idMagazynu);
        Task<byte[]> ExportRaportStanowPdfAsync(string? searchTerm, int? idMagazynu);
        Task<RaportRuchyMagazynoweDto> GetRaportRuchowAsync(string? searchTerm, int? idMagazynu, Data.Data.Magazyn.TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo);
        Task<byte[]> ExportRaportRuchowExcelAsync(string? searchTerm, int? idMagazynu, Data.Data.Magazyn.TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo);
        Task<byte[]> ExportRaportRuchowPdfAsync(string? searchTerm, int? idMagazynu, Data.Data.Magazyn.TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo);
    }
}
