namespace Interfaces.Magazyn.Dtos
{
    public class WydrukDokumentuResultDto
    {
        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = "application/octet-stream";

        public byte[] Content { get; set; } = [];

        public bool UzytoSzablonuAwaryjnego { get; set; }

        public string? KomunikatInformacyjny { get; set; }
    }
}
