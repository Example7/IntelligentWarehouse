using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RegulaAlertuDeleteDto
    {
        public RegulaAlertu Regula { get; set; } = null!;
        public int LiczbaAlertow { get; set; }
        public bool CzyMoznaUsunac => LiczbaAlertow == 0;
    }
}
