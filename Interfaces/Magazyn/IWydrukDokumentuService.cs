using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IWydrukDokumentuService
    {
        Task<WydrukDokumentuResultDto> GenerujWydrukWzAsync(int idDokumentuWz, int? idSzablonu = null);

        Task<WydrukDokumentuResultDto> GenerujWydrukPzAsync(int idDokumentuPz, int? idSzablonu = null);

        Task<WydrukDokumentuResultDto> GenerujWydrukMmAsync(int idDokumentuMm, int? idSzablonu = null);
    }
}
