using Interfaces.Magazyn.Dtos;

namespace Interfaces.Magazyn
{
    public interface IJednostkaMiaryService
    {
        Task<JednostkaMiaryIndexDto> GetIndexDataAsync(string? searchTerm);
        Task<JednostkaMiaryDetailsDto?> GetDetailsDataAsync(int idJednostki);
        Task<JednostkaMiaryDeleteDto?> GetDeleteDataAsync(int idJednostki);
        Task<int> GetAssignedProductsCountAsync(int idJednostki);
    }
}
