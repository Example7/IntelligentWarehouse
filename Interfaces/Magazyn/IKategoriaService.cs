using Data.Data.Magazyn;

namespace Interfaces.Magazyn
{
    public interface IKategoriaService
    {
        Task<IList<Kategoria>> GetKategorie();
    }
}
