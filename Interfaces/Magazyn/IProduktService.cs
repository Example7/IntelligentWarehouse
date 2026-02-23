using Data.Data.Magazyn;

namespace Interfaces.Magazyn
{
    public interface IProduktService
    {
        Task <IList<Produkt>> GetProdukty(int idKategorii);
        
        Task <Produkt?> GetProdukt(int? idProduktu);
    }
}
