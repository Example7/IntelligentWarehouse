namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaCreateShortagePzResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? DokumentPzId { get; set; }
        public string? DokumentPzNumer { get; set; }

        public static RezerwacjaCreateShortagePzResultDto Fail(string message)
            => new() { Success = false, Message = message };

        public static RezerwacjaCreateShortagePzResultDto Ok(int dokumentPzId, string dokumentPzNumer, string message)
            => new()
            {
                Success = true,
                DokumentPzId = dokumentPzId,
                DokumentPzNumer = dokumentPzNumer,
                Message = message
            };
    }
}
