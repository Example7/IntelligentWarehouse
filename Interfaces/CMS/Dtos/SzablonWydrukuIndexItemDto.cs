namespace Interfaces.CMS.Dtos
{
    public class SzablonWydrukuIndexItemDto
    {
        public int Id { get; set; }

        public string TypDokumentu { get; set; } = string.Empty;

        public string Nazwa { get; set; } = string.Empty;

        public string Wersja { get; set; } = string.Empty;

        public string NazwaPliku { get; set; } = string.Empty;

        public string Sciezka { get; set; } = string.Empty;

        public bool CzyAktywny { get; set; }

        public DateTime WgranoUtc { get; set; }

        public string? WgralEmail { get; set; }
    }
}
