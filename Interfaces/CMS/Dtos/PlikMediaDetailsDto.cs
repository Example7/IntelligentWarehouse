namespace Interfaces.CMS.Dtos
{
    public class PlikMediaDetailsDto
    {
        public long Id { get; set; }

        public string NazwaPliku { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string Sciezka { get; set; } = string.Empty;

        public long RozmiarBajty { get; set; }

        public string? Opis { get; set; }

        public DateTime WgranoUtc { get; set; }

        public int? WgralUserId { get; set; }

        public string? WgralEmail { get; set; }
    }
}
