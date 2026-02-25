namespace Interfaces.CMS.Dtos
{
    public class AktualnoscDetailsDto
    {
        public int IdAktualnosci { get; set; }

        public string TytulLinku { get; set; } = string.Empty;

        public string Nazwa { get; set; } = string.Empty;

        public string Tresc { get; set; } = string.Empty;

        public int Pozycja { get; set; }
    }
}
