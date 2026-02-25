namespace Interfaces.CMS.Dtos
{
    public class StronaIndexItemDto
    {
        public int IdStrony { get; set; }

        public string TytulLinku { get; set; } = string.Empty;

        public string Nazwa { get; set; } = string.Empty;

        public string Tresc { get; set; } = string.Empty;

        public int Pozycja { get; set; }
    }
}
