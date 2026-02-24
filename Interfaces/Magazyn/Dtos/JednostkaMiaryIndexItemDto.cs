using JednostkaMiaryEntity = Data.Data.Magazyn.JednostkaMiary;

namespace Interfaces.Magazyn.Dtos
{
    public class JednostkaMiaryIndexItemDto
    {
        public JednostkaMiaryEntity JednostkaMiary { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
    }
}
