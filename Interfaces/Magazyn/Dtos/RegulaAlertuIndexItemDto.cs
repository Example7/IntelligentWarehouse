using Data.Data.Magazyn;

namespace Interfaces.Magazyn.Dtos
{
    public class RegulaAlertuIndexItemDto
    {
        public RegulaAlertu Regula { get; set; } = null!;
        public int LiczbaAlertow { get; set; }
        public int LiczbaAlertowNiepotwierdzonych { get; set; }
    }
}
