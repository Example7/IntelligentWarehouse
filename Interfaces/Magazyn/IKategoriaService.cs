using Data.Data.Magazyn;
using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IKategoriaService
    {
        Task<IList<Kategoria>> GetKategorie();
        Task<KategoriaDetailsDto?> GetDetailsDataAsync(int idKategorii);
        Task<KategoriaDeleteDto?> GetDeleteDataAsync(int idKategorii);
        Task<(int liczbaProduktow, int liczbaPodkategorii)> GetDeleteBlockersAsync(int idKategorii);
    }
}
