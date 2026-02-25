namespace Interfaces.CMS.Dtos
{
    public class ZalacznikDokumentuDetailsDto
    {
        public long Id { get; set; }

        public string TypDokumentu { get; set; } = string.Empty;

        public int IdDokumentu { get; set; }

        public string NazwaPliku { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string Sciezka { get; set; } = string.Empty;

        public DateTime WgranoUtc { get; set; }

        public int? WgralUserId { get; set; }

        public string? WgralEmail { get; set; }
    }
}
