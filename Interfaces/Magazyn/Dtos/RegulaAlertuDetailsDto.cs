using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RegulaAlertuDetailsDto
    {
        public RegulaAlertu Regula { get; set; } = null!;
        public int LiczbaAlertow { get; set; }
        public int LiczbaAlertowNiepotwierdzonych { get; set; }
        public IList<Alert> OstatnieAlerty { get; set; } = new List<Alert>();
    }
}
