namespace Interfaces.Magazyn.Dtos
{
    public class AlertGenerateFromRulesResultDto
    {
        public int LiczbaPrzetworzonychRegul { get; set; }
        public int LiczbaSprawdzonychPozycji { get; set; }
        public int LiczbaNowychAlertow { get; set; }
        public int LiczbaAutoPotwierdzonychAlertow { get; set; }
        public int LiczbaPominietychDuplikatow { get; set; }
        public int LiczbaPominietychNieobslugiwanychRegul { get; set; }
    }
}
