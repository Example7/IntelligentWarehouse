using Data.Data.Magazyn;
using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IProduktService
    {
        Task<IList<Produkt>> GetProdukty(int idKategorii);
        Task<Produkt?> GetProdukt(int? idProduktu);
        Task<ProduktDetailsDto?> GetDetailsDataAsync(int idProduktu);
        Task<ProduktDeleteDto?> GetDeleteDataAsync(int idProduktu);
    }
}
