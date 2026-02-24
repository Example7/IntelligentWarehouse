using JednostkaMiaryEntity = Data.Data.Magazyn.JednostkaMiary;

namespace Interfaces.Magazyn.Dtos
{
    public class JednostkaMiaryDeleteDto
    {
        public JednostkaMiaryEntity JednostkaMiary { get; set; } = null!;
        public int LiczbaProduktow { get; set; }
        public bool CzyMoznaUsunac { get; set; }
    }
}
